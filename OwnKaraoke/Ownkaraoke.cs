using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace OwnKaraoke
{
    /// <summary>
    /// A custom Avalonia control that displays karaoke text with timed highlighting and smooth scrolling animations.
    /// </summary>
    public partial class OwnKaraokeDisplay : Control
    {
        #region Constants

        /// <summary>
        /// Factor used to calculate the width of space characters relative to font size.
        /// </summary>
        private const double FIXED_SPACE_WIDTH_FACTOR = 0.3;

        /// <summary>
        /// Speed of line position animation in pixels per millisecond.
        /// </summary>
        private const double LINE_ANIMATION_SPEED = 0.15;

        /// <summary>
        /// Speed of opacity animation in opacity units per millisecond.
        /// </summary>
        private const double OPACITY_ANIMATION_SPEED = 0.00076;

        #endregion

        #region Private Fields

        /// <summary>
        /// Internal collection of timed text elements.
        /// </summary>
        private readonly List<TimedTextElement> _itemsSourceInternal = new();

        /// <summary>
        /// Collection of display lines currently being rendered.
        /// </summary>
        private readonly List<KaraokeLine> _displayLines = new();

        /// <summary>
        /// Cache for FormattedText objects to improve performance.
        /// </summary>
        private readonly Dictionary<FormattedTextKey, FormattedText> _formattedTextCache = new();

        /// <summary>
        /// Reusable StringBuilder for building line text.
        /// </summary>
        private readonly System.Text.StringBuilder _stringBuilder = new();

        /// <summary>
        /// Subscription to the animation timer.
        /// </summary>
        private IDisposable? _animationSubscription;

        /// <summary>
        /// Timestamp of the last animation frame.
        /// </summary>
        private TimeSpan _lastFrameTime;

        /// <summary>
        /// Index of the currently highlighted syllable in the global sequence.
        /// </summary>
        private int _currentGlobalSyllableIndex;

        /// <summary>
        /// Total elapsed time in milliseconds since animation started.
        /// </summary>
        private double _timeElapsedInCurrentSyllableMs;

        /// <summary>
        /// Index of the first syllable used for line building after scrolling.
        /// </summary>
        private int _firstSyllableIndexForLineBuilding;

        /// <summary>
        /// Flag indicating whether line animations are currently running.
        /// </summary>
        private bool _isAnimatingLines;

        /// <summary>
        /// Cached typeface to avoid repeated creation.
        /// </summary>
        private Typeface? _cachedTypeface = null;

        /// <summary>
        /// Last font family used for typeface caching.
        /// </summary>
        private FontFamily? _lastFontFamily;

        /// <summary>
        /// Last font style used for typeface caching.
        /// </summary>
        private FontStyle _lastFontStyle;

        /// <summary>
        /// Last font weight used for typeface caching.
        /// </summary>
        private FontWeight _lastFontWeight;

        /// <summary>
        /// Gets a value indicating whether this control is attached to the visual tree.
        /// </summary>
        protected bool IsAttachedToVisualTree { get; private set; }

        #endregion
         
        #region Constructor and Initialization

        /// <summary>
        /// Initializes a new instance of the <see cref="OwnKaraokeDisplay"/> class.
        /// </summary>
        public OwnKaraokeDisplay()
        {
            SubscribeToPropertyChanges();
        }

        /// <summary>
        /// Subscribes to property changes to handle updates appropriately.
        /// </summary>
        private void SubscribeToPropertyChanges()
        {
            this.GetObservable(ItemsSourceProperty).Subscribe(HandleItemsSourceChanged);

            // Properties that require line rebuilding
            this.GetObservable(VisibleLinesCountProperty).Subscribe(_ => HandleVisualPropertyChanged());
            this.GetObservable(FontSizeProperty).Subscribe(_ => HandleVisualPropertyChanged());
            this.GetObservable(FontFamilyProperty).Subscribe(_ => HandleVisualPropertyChanged());
            this.GetObservable(FontWeightProperty).Subscribe(_ => HandleVisualPropertyChanged());
            this.GetObservable(FontStyleProperty).Subscribe(_ => HandleVisualPropertyChanged());
            this.GetObservable(BoundsProperty).Subscribe(_ => HandleVisualPropertyChanged());

            // Properties that only require visual refresh
            this.GetObservable(HighlightBrushProperty).Subscribe(_ => InvalidateVisual());
            this.GetObservable(AlreadySungBrushProperty).Subscribe(_ => InvalidateVisual());
            this.GetObservable(ForegroundProperty).Subscribe(_ => InvalidateVisual());
            this.GetObservable(TextAlignmentProperty).Subscribe(_ => InvalidateVisual());

            this.GetObservable(TempoProperty).Subscribe(HandleTempoChanged);
        }

        #endregion

        #region Timing and Duration Calculations

        /// <summary>
        /// Handles changes to the Tempo property during playback.
        /// </summary>
        /// <param name="newTempo">The new tempo value.</param>
        private void HandleTempoChanged(double newTempo) { }

        /// <summary>
        /// Calculates the duration for highlighting a specific syllable.
        /// </summary>
        /// <param name="syllableIndex">The index of the syllable.</param>
        /// <returns>The duration in milliseconds for highlighting the syllable.</returns>
        private double CalculateSyllableDuration(int syllableIndex)
        {
            if (syllableIndex >= _itemsSourceInternal.Count - 1)
                return 1000;

            var currentElement = _itemsSourceInternal[syllableIndex];
            var nextElement = _itemsSourceInternal[syllableIndex + 1];

            var timeDifference = nextElement.StartTimeMs - currentElement.StartTimeMs;
            var duration = timeDifference * 0.75;

            return Math.Min(duration, 1000);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles changes to the ItemsSource property.
        /// </summary>
        /// <param name="newValue">The new ItemsSource value.</param>
        private void HandleItemsSourceChanged(IEnumerable<TimedTextElement>? newValue)
        {
            _itemsSourceInternal.Clear();
            if (newValue != null)
                _itemsSourceInternal.AddRange(newValue);

            ClearFormattedTextCache();
            ResetAndBuildLines();
        }

        /// <summary>
        /// Handles changes to visual properties that require rebuilding lines.
        /// </summary>
        private void HandleVisualPropertyChanged()
        {
            _cachedTypeface = null;
            ClearFormattedTextCache();

            if (IsAttachedToVisualTree)
                BuildLines();
        }

        /// <summary>
        /// Resets animation state and rebuilds all lines.
        /// </summary>
        private void ResetAndBuildLines()
        {
            StopAnimation();
            _currentGlobalSyllableIndex = 0;
            _timeElapsedInCurrentSyllableMs = 0;
            _firstSyllableIndexForLineBuilding = 0;
            _isAnimatingLines = false;

            BuildLines();
            StartAnimation();
        }

        #endregion

        #region Visual Tree Lifecycle

        /// <summary>
        /// Called when the control is attached to the visual tree.
        /// </summary>
        /// <param name="e">The attachment event arguments.</param>
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            IsAttachedToVisualTree = true;

            _itemsSourceInternal.Clear();
            if (ItemsSource != null)
                _itemsSourceInternal.AddRange(ItemsSource);

            ResetAndBuildLines();
        }

        /// <summary>
        /// Called when the control is detached from the visual tree.
        /// </summary>
        /// <param name="e">The detachment event arguments.</param>
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            IsAttachedToVisualTree = false;
            StopAnimation();
            ClearFormattedTextCache();
        }

        #endregion

        #region Tempo Calculations

        /// <summary>
        /// Calculates the tempo multiplier from the Tempo property.
        /// </summary>
        /// <returns>The tempo multiplier (1.0 = normal speed, 1.1 = 10% faster, 0.9 = 10% slower)</returns>
        private double GetTempoMultiplier()
        {
            // Tempo range: -2.0 to +2.0
            // Each 0.1 = 10% change
            // Formula: 1.0 + (Tempo * 1.0) = multiplier
            return 1.0 + Tempo;
        }

        /// <summary>
        /// Applies tempo scaling to a time value.
        /// </summary>
        /// <param name="originalTimeMs">The original time in milliseconds.</param>
        /// <returns>The tempo-adjusted time in milliseconds.</returns>
        private double ApplyTempoToTime(double originalTimeMs)
        {
            var multiplier = GetTempoMultiplier();
            return originalTimeMs / multiplier;
        }

        /// <summary>
        /// Applies tempo scaling to elapsed time during animation.
        /// </summary>
        /// <param name="elapsedMs">The elapsed time in milliseconds.</param>
        /// <returns>The tempo-adjusted elapsed time.</returns>
        private double ApplyTempoToElapsedTime(double elapsedMs)
        {
            var multiplier = GetTempoMultiplier();
            return elapsedMs * multiplier;
        }

        #endregion
    }
}
