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

        #endregion
    }
}
