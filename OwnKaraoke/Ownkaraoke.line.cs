using Avalonia.Controls;
using System;
using System.Linq;

namespace OwnKaraoke
{
    public partial class OwnKaraokeDisplay : Control
    {
        #region Line Building

        /// <summary>
        /// Builds all display lines from the current syllable positions.
        /// </summary>
        private void BuildLines()
        {
            var existingLines = _displayLines.ToList();
            var isScrollAnimation = _isAnimatingLines && existingLines.Count > 0;

            _displayLines.Clear();

            if (!CanBuildLines())
            {
                InvalidateVisual();
                return;
            }

            var availableWidth = Bounds.Width;
            var actualFixedSpaceWidth = FontSize * FIXED_SPACE_WIDTH_FACTOR;
            var currentSyllableIdx = _firstSyllableIndexForLineBuilding;
            var currentY = 0.0;

            for (var lineNum = 0; lineNum < VisibleLinesCount && currentSyllableIdx < _itemsSourceInternal.Count; lineNum++)
            {
                var line = BuildSingleLine(ref currentSyllableIdx, availableWidth, actualFixedSpaceWidth, currentY);

                if (line != null)
                {
                    SetLineAnimationPosition(line, lineNum, existingLines, isScrollAnimation);
                    _displayLines.Add(line);
                    currentY += line.LineHeight;
                }

                if (currentSyllableIdx >= _itemsSourceInternal.Count)
                    break;
            }

            InvalidateMeasure();
            InvalidateVisual();
        }

        /// <summary>
        /// Determines if lines can be built based on current state.
        /// </summary>
        /// <returns>True if lines can be built; otherwise, false.</returns>
        private bool CanBuildLines() =>
            _itemsSourceInternal.Count > 0 && VisibleLinesCount > 0 &&
            Bounds.Width > 0 && IsAttachedToVisualTree;

        /// <summary>
        /// Builds a single line of karaoke text.
        /// </summary>
        /// <param name="currentSyllableIdx">The current syllable index (modified by reference).</param>
        /// <param name="availableWidth">The available width for the line.</param>
        /// <param name="actualFixedSpaceWidth">The width of space characters.</param>
        /// <param name="targetY">The target Y position for the line.</param>
        /// <returns>A new KaraokeLine object or null if no line could be built.</returns>
        private KaraokeLine? BuildSingleLine(ref int currentSyllableIdx, double availableWidth,
            double actualFixedSpaceWidth, double targetY)
        {
            if (currentSyllableIdx >= _itemsSourceInternal.Count)
                return null;

            var line = new KaraokeLine { FirstSyllableGlobalIndex = currentSyllableIdx, TargetY = targetY };
            var currentLineWidth = 0.0;
            var maxLineHeight = 0.0;
            var lineBreakEncountered = false;

            _stringBuilder.Clear();

            while (currentSyllableIdx < _itemsSourceInternal.Count)
            {
                var element = _itemsSourceInternal[currentSyllableIdx];
                var originalText = element.Text ?? "";

                if (originalText == "._.")
                {
                    currentSyllableIdx++;
                    lineBreakEncountered = true;
                    break;
                }

                var (syllableWidth, syllableHeight, formattedText) =
                    CalculateSyllableMetrics(originalText, actualFixedSpaceWidth);

                if (!CanFitSyllable(line, syllableWidth, currentLineWidth, availableWidth))
                    break;

                AddSyllableToLine(line, element, currentSyllableIdx, formattedText,
                    syllableWidth, syllableHeight, currentLineWidth, originalText);

                currentLineWidth += syllableWidth;
                maxLineHeight = Math.Max(maxLineHeight, syllableHeight);
                currentSyllableIdx++;
            }

            return FinalizeLine(line, lineBreakEncountered, maxLineHeight);
        }

        #endregion
    }
}
