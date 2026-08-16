using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System.Collections.Generic;

namespace OwnKaraoke
{
    public partial class OwnKaraokeDisplay : Control
    {
        #region Styled Properties

        /// <summary>
        /// Defines the ItemsSource property.
        /// </summary>
        public static readonly StyledProperty<IEnumerable<TimedTextElement>?> ItemsSourceProperty =
            AvaloniaProperty.Register<OwnKaraokeDisplay, IEnumerable<TimedTextElement>?>(nameof(ItemsSource));

        /// <summary>
        /// Defines the ChordSource property.
        /// </summary>
        public static readonly StyledProperty<IEnumerable<TimedChordElement>?> ChordSourceProperty =
            AvaloniaProperty.Register<OwnKaraokeDisplay, IEnumerable<TimedChordElement>?>(nameof(ChordSource));

        /// <summary>
        /// Defines the ChordBrush property.
        /// </summary>
        public static readonly StyledProperty<IBrush?> ChordBrushProperty =
            AvaloniaProperty.Register<OwnKaraokeDisplay, IBrush?>(nameof(ChordBrush), Brushes.Orange);

        /// <summary>
        /// Defines the ChordFontScale property.
        /// </summary>
        public static readonly StyledProperty<double> ChordFontScaleProperty =
            AvaloniaProperty.Register<OwnKaraokeDisplay, double>(nameof(ChordFontScale), 0.45,
                validate: value => value > 0.0 && value <= 1.0);

        /// <summary>
        /// Defines the VisibleLinesCount property.
        /// </summary>
        public static readonly StyledProperty<int> VisibleLinesCountProperty =
            AvaloniaProperty.Register<OwnKaraokeDisplay, int>(nameof(VisibleLinesCount), 3, validate: value => value > 0);

        /// <summary>
        /// Defines the HighlightBrush property.
        /// </summary>
        public static readonly StyledProperty<IBrush?> HighlightBrushProperty =
            AvaloniaProperty.Register<OwnKaraokeDisplay, IBrush?>(nameof(HighlightBrush), Brushes.Yellow);

        /// <summary>
        /// Defines the AlreadySungBrush property.
        /// </summary>
        public static readonly StyledProperty<IBrush?> AlreadySungBrushProperty =
            AvaloniaProperty.Register<OwnKaraokeDisplay, IBrush?>(nameof(AlreadySungBrush), Brushes.LightGoldenrodYellow);

        /// <summary>
        /// Defines the Foreground property.
        /// </summary>
        public static readonly StyledProperty<IBrush?> ForegroundProperty =
            TextBlock.ForegroundProperty.AddOwner<OwnKaraokeDisplay>();

        /// <summary>
        /// Defines the FontSize property.
        /// </summary>
        public static readonly StyledProperty<double> FontSizeProperty =
            TextBlock.FontSizeProperty.AddOwner<OwnKaraokeDisplay>();

        /// <summary>
        /// Defines the FontFamily property.
        /// </summary>
        public static readonly StyledProperty<FontFamily> FontFamilyProperty =
            TextBlock.FontFamilyProperty.AddOwner<OwnKaraokeDisplay>();

        /// <summary>
        /// Defines the FontWeight property.
        /// </summary>
        public static readonly StyledProperty<FontWeight> FontWeightProperty =
            TextBlock.FontWeightProperty.AddOwner<OwnKaraokeDisplay>();

        /// <summary>
        /// Defines the FontStyle property.
        /// </summary>
        public static readonly StyledProperty<FontStyle> FontStyleProperty =
            TextBlock.FontStyleProperty.AddOwner<OwnKaraokeDisplay>();

        /// <summary>
        /// Defines the TextAlignment property.
        /// </summary>
        public static readonly StyledProperty<TextAlignment> TextAlignmentProperty =
            TextBlock.TextAlignmentProperty.AddOwner<OwnKaraokeDisplay>();

        #endregion

        #region Property Accessors

        /// <summary>
        /// Gets or sets the source collection of timed text elements to display.
        /// </summary>
        public IEnumerable<TimedTextElement>? ItemsSource
        {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        /// <summary>
        /// Chords to hang above the lyrics. Leave it null and the display works exactly as
        /// it did before — no chord row, no extra line height.
        /// </summary>
        public IEnumerable<TimedChordElement>? ChordSource
        {
            get => GetValue(ChordSourceProperty);
            set => SetValue(ChordSourceProperty, value);
        }

        /// <summary>
        /// Colour of the chord labels.
        /// </summary>
        public IBrush? ChordBrush
        {
            get => GetValue(ChordBrushProperty);
            set => SetValue(ChordBrushProperty, value);
        }

        /// <summary>
        /// Chord label size as a fraction of <see cref="FontSize"/>, 0–1.
        /// </summary>
        public double ChordFontScale
        {
            get => GetValue(ChordFontScaleProperty);
            set => SetValue(ChordFontScaleProperty, value);
        }

        /// <summary>
        /// Gets or sets the number of lines visible at once in the display.
        /// </summary>
        public int VisibleLinesCount
        {
            get => GetValue(VisibleLinesCountProperty);
            set => SetValue(VisibleLinesCountProperty, value);
        }

        /// <summary>
        /// Gets or sets the brush used to highlight the currently singing syllable.
        /// </summary>
        public IBrush? HighlightBrush
        {
            get => GetValue(HighlightBrushProperty);
            set => SetValue(HighlightBrushProperty, value);
        }

        /// <summary>
        /// Gets or sets the brush used to display already sung syllables.
        /// </summary>
        public IBrush? AlreadySungBrush
        {
            get => GetValue(AlreadySungBrushProperty);
            set => SetValue(AlreadySungBrushProperty, value);
        }

        /// <summary>
        /// Gets or sets the foreground brush for normal text.
        /// </summary>
        public IBrush? Foreground
        {
            get => GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the font size of the text.
        /// </summary>
        public double FontSize
        {
            get => GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the font family of the text.
        /// </summary>
        public FontFamily FontFamily
        {
            get => GetValue(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }

        /// <summary>
        /// Gets or sets the font weight of the text.
        /// </summary>
        public FontWeight FontWeight
        {
            get => GetValue(FontWeightProperty);
            set => SetValue(FontWeightProperty, value);
        }

        /// <summary>
        /// Gets or sets the font style of the text.
        /// </summary>
        public FontStyle FontStyle
        {
            get => GetValue(FontStyleProperty);
            set => SetValue(FontStyleProperty, value);
        }

        /// <summary>
        /// Gets or sets the text alignment of the lines.
        /// </summary>
        public TextAlignment TextAlignment
        {
            get => GetValue(TextAlignmentProperty);
            set => SetValue(TextAlignmentProperty, value);
        }

        #region Tempo Property

        /// <summary>
        /// Defines the Tempo property for controlling playback speed.
        /// Range: -2.0 to +2.0, where each 0.1 represents 10% speed change.
        /// 0.0 = normal speed, 0.1 = 10% faster, -0.1 = 10% slower
        /// </summary>
        public static readonly StyledProperty<double> TempoProperty =
            AvaloniaProperty.Register<OwnKaraokeDisplay, double>(
                nameof(Tempo),
                0.0,
                validate: value => value >= -2.0 && value <= 2.0);

        /// <summary>
        /// Gets or sets the tempo (speed) of the karaoke playback.
        /// Range: -2.0 to +2.0, where each 0.1 represents 10% speed change.
        /// </summary>
        public double Tempo
        {
            get => GetValue(TempoProperty);
            set => SetValue(TempoProperty, value);
        }

        #endregion

        #region External Position Property

        /// <summary>
        /// Defines the UseExternalPosition property.
        /// </summary>
        public static readonly StyledProperty<bool> UseExternalPositionProperty =
            AvaloniaProperty.Register<OwnKaraokeDisplay, bool>(nameof(UseExternalPosition), false);

        /// <summary>
        /// Gets or sets whether the karaoke position is driven by an external source
        /// (e.g. an audio player) instead of the internal timer.
        /// When true, call <see cref="UpdatePosition"/> on each position update from the external source.
        /// </summary>
        public bool UseExternalPosition
        {
            get => GetValue(UseExternalPositionProperty);
            set => SetValue(UseExternalPositionProperty, value);
        }

        #endregion

        #region LyricOffset Property

        /// <summary>
        /// Defines the LyricOffset property.
        /// </summary>
        public static readonly StyledProperty<double> LyricOffsetProperty =
            AvaloniaProperty.Register<OwnKaraokeDisplay, double>(nameof(LyricOffset), 0.0);

        /// <summary>
        /// Gets or sets the lyric display offset in milliseconds relative to the playback position.
        /// Positive values show the lyrics earlier (ahead of the audio),
        /// negative values show them later (behind the audio).
        /// Example: +100 means lyrics are highlighted 100 ms before the audio reaches that point.
        /// Example: -100 means lyrics are highlighted 100 ms after the audio passes that point.
        /// </summary>
        public double LyricOffset
        {
            get => GetValue(LyricOffsetProperty);
            set => SetValue(LyricOffsetProperty, value);
        }

        #endregion

        #endregion

        #region Status and Position Properties

        /// <summary>
        /// Defines the Status property.
        /// </summary>
        public static readonly StyledProperty<KaraokeStatus> StatusProperty =
            AvaloniaProperty.Register<OwnKaraokeDisplay, KaraokeStatus>(nameof(Status), KaraokeStatus.Idle);

        /// <summary>
        /// Defines the Duration property.
        /// </summary>
        public static readonly StyledProperty<double> DurationProperty =
            AvaloniaProperty.Register<OwnKaraokeDisplay, double>(nameof(Duration), 0.0);

        /// <summary>
        /// Defines the OriginalDuration property.
        /// </summary>
        public static readonly StyledProperty<double> OriginalDurationProperty =
            AvaloniaProperty.Register<OwnKaraokeDisplay, double>(nameof(OriginalDuration), 0.0);

        /// <summary>
        /// Gets the current status of the karaoke control.
        /// </summary>
        public KaraokeStatus Status
        {
            get => GetValue(StatusProperty);
            private set => SetValue(StatusProperty, value);
        }

        /// <summary>
        /// Gets the current playback position in milliseconds (tempo-adjusted).
        /// </summary>
        public double Position
        {
            get => _position;
            private set => _position = value;
        }

        /// <summary>
        /// Gets the total duration of the karaoke content in milliseconds (tempo-adjusted).
        /// </summary>
        public double Duration
        {
            get => GetValue(DurationProperty);
            private set => SetValue(DurationProperty, value);
        }

        /// <summary>
        /// Gets the current playback position in original milliseconds (without tempo adjustment).
        /// </summary>
        public double OriginalPosition
        {
            get => _originalPosition;
            private set => _originalPosition = value;
        }

        /// <summary>
        /// Gets the total duration in original milliseconds (without tempo adjustment).
        /// </summary>
        public double OriginalDuration
        {
            get => GetValue(OriginalDurationProperty);
            private set => SetValue(OriginalDurationProperty, value);
        }

        #endregion
    }
}
