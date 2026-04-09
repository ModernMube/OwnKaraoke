using Avalonia.Controls;
using System.Linq;

namespace OwnKaraoke
{
    public partial class OwnKaraokeDisplay : Control
    {
        #region Scrolling Logic

        /// <summary>
        /// Checks if scrolling is necessary to display new lines.
        /// Improved logic: checks multiple conditions and handles short lines.
        /// </summary>
        private void CheckForLineScroll()
        {
            if (_displayLines.Count < 2)
                return;

            var shouldScroll = ShouldScrollToNextLine();

            if (shouldScroll)
            {
                var secondLine = _displayLines[1];
                _firstSyllableIndexForLineBuilding = secondLine.FirstSyllableGlobalIndex;
                ScrollLinesUp();
                RebuildAllLinesAfterScroll();
            }
        }

        /// <summary>
        /// Determines if scrolling is necessary.
        /// Improved logic: takes multiple conditions into account.
        /// </summary>
        private bool ShouldScrollToNextLine()
        {
            if (_displayLines.Count < 2)
                return false;

            var firstLine = _displayLines[0];
            var secondLine = _displayLines[1];

            // 1. Check: Is the first line fully processed?
            if (firstLine.IsFullyProcessed(_currentGlobalSyllableIndex))
                return true;

            // 2. Check: Is the current syllable in the second line?
            if (_currentGlobalSyllableIndex >= secondLine.FirstSyllableGlobalIndex &&
                _currentGlobalSyllableIndex <= secondLine.LastSyllableGlobalIndex)
            {
                // 3a. Handling short lines: If the second line contains few syllables (< 4)
                var syllablesInSecondLine = secondLine.Syllables.Count;
                if (syllablesInSecondLine > 0 && syllablesInSecondLine < 4)
                {
                    // For short lines, we scroll at the first syllable
                    return true;
                }

                // 3b. Normal lines: 1/3 rule, but minimum 2 syllables
                if (syllablesInSecondLine >= 4)
                {
                    var currentSyllableInSecondLine = secondLine.Syllables.FirstOrDefault(s =>
                        s.GlobalIndex == _currentGlobalSyllableIndex);

                    if (currentSyllableInSecondLine != null)
                    {
                        var syllableIndexInLine = secondLine.Syllables.IndexOf(currentSyllableInSecondLine);
                        var oneThirdPosition = Math.Max(1, syllablesInSecondLine / 3.0);

                        return syllableIndexInLine >= oneThirdPosition;
                    }
                }
            }

            // 4. Check: Is the current syllable after the second line?
            if (_currentGlobalSyllableIndex > secondLine.LastSyllableGlobalIndex)
                return true;

            return false;
        }

        /// <summary>
        /// Checks and performs necessary scrolling after seeking.
        /// New method: ensures that the target syllable is visible.
        /// </summary>
        private void CheckScrollingAfterSeek(int targetSyllableIndex)
        {
            if (_displayLines.Count == 0 || targetSyllableIndex >= _itemsSourceInternal.Count)
                return;

            // Find which line contains the target syllable
            var targetLineIndex = FindLineContainingSyllable(targetSyllableIndex);

            // If the target syllable is not visible or at the end/after the second line
            if (targetLineIndex == -1 || targetLineIndex >= VisibleLinesCount - 1)
            {
                // Reset the line building starting point
                var optimalStartIndex = CalculateOptimalStartIndex(targetSyllableIndex);
                _firstSyllableIndexForLineBuilding = optimalStartIndex;

                // Rebuild the lines
                BuildLines();
            }
            else if (targetLineIndex >= 1) // If it's in the second or later line
            {
                // Check if we need to scroll based on position in the second line
                var secondLine = _displayLines[1];
                if (targetSyllableIndex >= secondLine.FirstSyllableGlobalIndex)
                {
                    var syllableInSecondLine = secondLine.Syllables.FirstOrDefault(s =>
                        s.GlobalIndex == targetSyllableIndex);

                    if (syllableInSecondLine != null)
                    {
                        var syllableIndexInLine = secondLine.Syllables.IndexOf(syllableInSecondLine);
                        var syllablesInSecondLine = secondLine.Syllables.Count;

                        // For short lines (< 4 syllables) or if too far back in the line
                        if (syllablesInSecondLine < 4 ||
                            syllableIndexInLine >= Math.Max(1, syllablesInSecondLine / 3.0))
                        {
                            _firstSyllableIndexForLineBuilding = secondLine.FirstSyllableGlobalIndex;
                            ScrollLinesUp();
                            RebuildAllLinesAfterScroll();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finds which visible line contains a given syllable.
        /// </summary>
        /// <param name="syllableIndex">The index of the syllable to find.</param>
        /// <returns>The line index (from 0), or -1 if not found.</returns>
        private int FindLineContainingSyllable(int syllableIndex)
        {
            for (int i = 0; i < _displayLines.Count; i++)
            {
                var line = _displayLines[i];
                if (syllableIndex >= line.FirstSyllableGlobalIndex &&
                    syllableIndex <= line.LastSyllableGlobalIndex)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Calculates the optimal starting index for line building based on a target syllable.
        /// </summary>
        /// <param name="targetSyllableIndex">The index of the target syllable.</param>
        /// <returns>The optimal starting index.</returns>
        private int CalculateOptimalStartIndex(int targetSyllableIndex)
        {
            if (targetSyllableIndex <= 0)
                return 0;

            var estimatedSyllablesPerLine = EstimateSyllablesPerLine();
            var totalVisibleSyllables = estimatedSyllablesPerLine * VisibleLinesCount;

            // The target syllable should be in the middle of the first line (not at the end)
            var targetPositionInFirstLine = estimatedSyllablesPerLine / 3;
            var optimalStart = Math.Max(0, targetSyllableIndex - targetPositionInFirstLine);

            // Ensure it's not too far from the end
            var maxStart = Math.Max(0, _itemsSourceInternal.Count - totalVisibleSyllables);

            return Math.Min(optimalStart, maxStart);
        }

        /// <summary>
        /// Rebuilds all lines after scrolling.
        /// </summary>
        private void RebuildAllLinesAfterScroll()
        {
            var existingLines = _displayLines.ToList();
            _displayLines.Clear();

            var availableWidth = Bounds.Width;
            var actualFixedSpaceWidth = FontSize * FIXED_SPACE_WIDTH_FACTOR;
            var currentSyllableIdx = _firstSyllableIndexForLineBuilding;
            var currentY = 0.0;

            for (var lineNum = 0; lineNum < VisibleLinesCount && currentSyllableIdx < _itemsSourceInternal.Count; lineNum++)
            {
                var line = BuildSingleLine(ref currentSyllableIdx, availableWidth, actualFixedSpaceWidth, currentY);

                if (line != null)
                {
                    if (lineNum < existingLines.Count)
                    {
                        line.CurrentY = existingLines[lineNum].CurrentY;
                        line.Opacity = existingLines[lineNum].Opacity;
                        line.TargetOpacity = existingLines[lineNum].TargetOpacity;
                    }
                    else
                    {
                        line.CurrentY = line.TargetY;
                        line.Opacity = 0.0;
                        line.TargetOpacity = 1.0;
                        _isAnimatingLines = true;
                    }

                    _displayLines.Add(line);
                    currentY += line.LineHeight;
                }
                else
                {
                    break;
                }
            }

            InvalidateMeasure();
            InvalidateVisual();
        }

        /// <summary>
        /// Determines if the highlight is at approximately one-third of the second line.
        /// Improved version: better handles short lines.
        /// </summary>
        /// <param name="secondLine">The second line.</param>
        /// <returns>True if the highlight is at the one-third position; otherwise false.</returns>
        private bool IsHighlightInSecondLineAtOneThird(KaraokeLine secondLine)
        {
            if (_currentGlobalSyllableIndex < secondLine.FirstSyllableGlobalIndex ||
                _currentGlobalSyllableIndex > secondLine.LastSyllableGlobalIndex)
                return false;

            var totalSyllablesInSecondLine = secondLine.Syllables.Count;
            if (totalSyllablesInSecondLine == 0)
                return false;

            // For short lines (less than 4 syllables), we scroll at the first syllable
            if (totalSyllablesInSecondLine < 4)
                return true;

            var currentSyllableInSecondLine = secondLine.Syllables.FirstOrDefault(s =>
                s.GlobalIndex == _currentGlobalSyllableIndex);

            if (currentSyllableInSecondLine == null)
                return false;

            var syllableIndexInLine = secondLine.Syllables.IndexOf(currentSyllableInSecondLine);
            var oneThirdPosition = Math.Max(1, totalSyllablesInSecondLine / 3.0);

            return syllableIndexInLine >= oneThirdPosition;
        }

        /// <summary>
        /// Scrolls all lines upward by removing the first line and adjusting positions.
        /// </summary>
        private void ScrollLinesUp()
        {
            if (_displayLines.Count == 0)
                return;

            var firstLineHeight = _displayLines[0].LineHeight;
            _displayLines.RemoveAt(0);

            foreach (var line in _displayLines)
            {
                line.TargetY -= firstLineHeight;
            }

            _isAnimatingLines = true;
        }

        /// <summary>
        /// Builds a new line at the bottom of the display.
        /// </summary>
        private void BuildNewBottomLine()
        {
            if (_displayLines.Count == 0)
                return;

            var lastLine = _displayLines[^1];
            var nextSyllableIndex = lastLine.LastSyllableGlobalIndex + 1;

            if (nextSyllableIndex >= _itemsSourceInternal.Count)
                return;

            var hasMoreContent = false;
            for (int i = nextSyllableIndex; i < _itemsSourceInternal.Count; i++)
            {
                if (!string.IsNullOrEmpty(_itemsSourceInternal[i].Text) && _itemsSourceInternal[i].Text != "._.")
                {
                    hasMoreContent = true;
                    break;
                }
            }

            if (!hasMoreContent)
                return;

            var newTargetY = lastLine.TargetY + lastLine.LineHeight;

            var availableWidth = Bounds.Width;
            var actualFixedSpaceWidth = FontSize * FIXED_SPACE_WIDTH_FACTOR;

            var tempSyllableIndex = nextSyllableIndex;
            var newLine = BuildSingleLine(ref tempSyllableIndex, availableWidth, actualFixedSpaceWidth, newTargetY);

            if (newLine != null)
            {
                newLine.CurrentY = lastLine.TargetY;
                newLine.Opacity = 0.0;
                newLine.TargetOpacity = 1.0;

                _displayLines.Add(newLine);
                _isAnimatingLines = true;

                if (newLine.Syllables.Count == 0 && tempSyllableIndex < _itemsSourceInternal.Count)
                {
                    BuildNewBottomLine();
                }
            }
        }

        #endregion
    }
}