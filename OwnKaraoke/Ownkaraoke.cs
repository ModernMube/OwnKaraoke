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

        /// <summary>
        /// The original elapsed time in milliseconds (without tempo adjustment).
        /// </summary>
        private double _originalElapsedTimeMs;

        /// <summary>
        /// The last seek position in milliseconds.
        /// </summary>
        private double _lastSeekPositionMs;

        /// <summary>
        /// The original time elapsed since the last seek operation.
        /// </summary>
        private double _timeSinceLastSeekMs;

        /// <summary>
        /// Indicates whether a seek operation is currently in progress.
        /// </summary>
        private bool _isSeeking;

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

        #region Timing and Duration Calculation

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
        /// Handles changes in the Tempo property during playback.
        /// IMPROVED version: Properly resynchronizes the animation.
        /// </summary>
        /// <param name="newTempo">The new tempo value.</param>
        private void HandleTempoChanged(double newTempo)
        {
            // Update the total duration for the new tempo
            UpdateDurationForTempo();

            // If playback is in progress, we need to resynchronize the timing
            if (Status == KaraokeStatus.Playing && _itemsSourceInternal.Count > 0)
            {
                // FIX: Recalculate the tempo-modified position from the current original time
                var currentOriginalPosition = _originalElapsedTimeMs;
                var newTempoAdjustedPosition = ApplyTempoToTime(currentOriginalPosition);

                // Update the Position property
                Position = newTempoAdjustedPosition;

                // CRITICAL FIX: Recalculate the _timeElapsedInCurrentSyllableMs value
                // This is what actually drives the animation
                _timeElapsedInCurrentSyllableMs = newTempoAdjustedPosition;

                // Check if we're in the correct syllable with the new tempo
                ValidateAndAdjustCurrentSyllable();
            }
        }

        /// <summary>
        /// Checks and sets the correct current syllable based on the new tempo.
        /// IMPROVED version: Properly handles syllable transitions.
        /// </summary>
        private void ValidateAndAdjustCurrentSyllable()
        {
            if (_itemsSourceInternal.Count == 0)
                return;

            var currentOriginalTime = _originalElapsedTimeMs;
            var newCorrectSyllableIndex = FindSyllableAtPosition(currentOriginalTime);

            // If the syllable index has changed, update it
            if (newCorrectSyllableIndex != _currentGlobalSyllableIndex)
            {
                _currentGlobalSyllableIndex = newCorrectSyllableIndex;

                // FIX: Recalculate the time elapsed in the syllable based on the new tempo
                if (_currentGlobalSyllableIndex < _itemsSourceInternal.Count)
                {
                    var currentSyllable = _itemsSourceInternal[_currentGlobalSyllableIndex];
                    var tempoAdjustedStartTime = ApplyTempoToTime(currentSyllable.StartTimeMs);
                    var tempoAdjustedCurrentTime = ApplyTempoToTime(currentOriginalTime);

                    // CRITICAL: Set _timeElapsedInCurrentSyllableMs based on the new tempo
                    _timeElapsedInCurrentSyllableMs = tempoAdjustedCurrentTime;
                }

                // Check if scrolling is needed
                if (_displayLines.Count > 0)
                {
                    CheckForLineScroll();
                }
            }
            else
            {
                // Same syllable, but recalculate the elapsed time within it based on the new tempo
                if (_currentGlobalSyllableIndex < _itemsSourceInternal.Count)
                {
                    var tempoAdjustedCurrentTime = ApplyTempoToTime(currentOriginalTime);

                    // CRITICAL: This value needs to be updated when tempo changes
                    _timeElapsedInCurrentSyllableMs = tempoAdjustedCurrentTime;
                }
            }
        }

        /// <summary>
        /// Handles changes to the ItemsSource property.
        /// </summary>
        /// <param name="newValue">The new ItemsSource value.</param>
        private void HandleItemsSourceChanged(IEnumerable<TimedTextElement>? newValue)
        {
            _itemsSourceInternal.Clear();
            if (newValue != null)
                _itemsSourceInternal.AddRange(newValue);

            CalculateDuration();
            ClearFormattedTextCache();

            if (_itemsSourceInternal.Count == 0)
            {
                Status = KaraokeStatus.Idle;
                OriginalPosition = 0;
                Position = 0;
            }

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
            _originalElapsedTimeMs = 0;
            _lastSeekPositionMs = 0;
            _timeSinceLastSeekMs = 0;

            BuildLines();

            if (_itemsSourceInternal.Count > 0 && Status == KaraokeStatus.Playing)
            {
                StartAnimation();
            }
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
            var multiplier = 1.0 + Tempo;

            // Safety check: not zero or negative
            return Math.Max(0.1, multiplier);
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