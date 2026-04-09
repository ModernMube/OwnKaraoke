using Avalonia.Media;
using System.Collections.Generic;

namespace OwnKaraoke
{
    /// <summary>
    /// Represents a line of karaoke text containing syllable metrics and formatting information.
    /// </summary>
    internal sealed class KaraokeLine
    {
        /// <summary>
        /// Gets the collection of syllables in this line.
        /// </summary>
        public List<SyllableMetrics> Syllables { get; } = new();

        /// <summary>
        /// Gets or sets the formatted text representation of the entire line.
        /// </summary>
        public FormattedText? FormattedTextLine { get; set; }

        /// <summary>
        /// Gets or sets the height of this line.
        /// </summary>
        public double LineHeight { get; set; }

        /// <summary>
        /// Gets or sets the width of this line.
        /// </summary>
        public double LineWidth { get; set; }

        /// <summary>
        /// Gets or sets the global index of the first syllable in this line.
        /// </summary>
        public int FirstSyllableGlobalIndex { get; set; } = -1;

        /// <summary>
        /// Gets or sets the global index of the last syllable in this line.
        /// </summary>
        public int LastSyllableGlobalIndex { get; set; } = -1;

        /// <summary>
        /// Gets or sets the target Y position for animation.
        /// </summary>
        public double TargetY { get; set; }

        /// <summary>
        /// Gets or sets the current Y position during animation.
        /// </summary>
        public double CurrentY { get; set; }

        /// <summary>
        /// Gets or sets the current opacity of the line.
        /// </summary>
        public double Opacity { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets the target opacity for animation.
        /// </summary>
        public double TargetOpacity { get; set; } = 1.0;

        /// <summary>
        /// Determines whether this line has been fully processed based on the current highlighted syllable index.
        /// </summary>
        /// <param name="globalHighlightedSyllableIndex">The current global syllable index being highlighted.</param>
        /// <returns>True if the line is fully processed; otherwise, false.</returns>
        public bool IsFullyProcessed(int globalHighlightedSyllableIndex) =>
            Syllables.Count == 0 || globalHighlightedSyllableIndex > LastSyllableGlobalIndex;
    }
}
