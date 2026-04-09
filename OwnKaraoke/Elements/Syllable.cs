using Avalonia.Media;

namespace OwnKaraoke
{
    /// <summary>
    /// Contains metrics and formatting information for a single syllable.
    /// </summary>
    internal sealed class SyllableMetrics
    {
        /// <summary>
        /// Gets the original timed text element this syllable represents.
        /// </summary>
        public TimedTextElement OriginalElement { get; }

        /// <summary>
        /// Gets the global index of this syllable in the entire sequence.
        /// </summary>
        public int GlobalIndex { get; }

        /// <summary>
        /// Gets the formatted text representation of this syllable.
        /// </summary>
        public FormattedText FormattedTextSyllable { get; }

        /// <summary>
        /// Gets or sets the X offset of this syllable within its line.
        /// </summary>
        public double XOffsetInLine { get; set; }

        /// <summary>
        /// Gets the width of this syllable.
        /// </summary>
        public double Width { get; }

        /// <summary>
        /// Gets the height of this syllable.
        /// </summary>
        public double Height { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyllableMetrics"/> class.
        /// </summary>
        /// <param name="originalElement">The original timed text element.</param>
        /// <param name="globalIndex">The global index of the syllable.</param>
        /// <param name="formattedTextSyllable">The formatted text representation.</param>
        /// <param name="width">The width of the syllable.</param>
        /// <param name="height">The height of the syllable.</param>
        public SyllableMetrics(TimedTextElement originalElement, int globalIndex,
            FormattedText formattedTextSyllable, double width, double height)
        {
            OriginalElement = originalElement;
            GlobalIndex = globalIndex;
            FormattedTextSyllable = formattedTextSyllable;
            Width = width;
            Height = height;
        }
    }
}
