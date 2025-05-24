using Avalonia.Controls;
using System.Linq;

namespace OwnKaraoke
{
    public partial class OwnKaraokeDisplay : Control
    {
        #region Scrolling Logic

        /// <summary>
        /// Checks if the display should scroll to show new lines.
        /// </summary>
        private void CheckForLineScroll()
        {
            if (_displayLines.Count < 2)
                return;

            var secondLine = _displayLines[1];
            if (IsHighlightInSecondLineAtOneThird(secondLine))
            {
                _firstSyllableIndexForLineBuilding = secondLine.FirstSyllableGlobalIndex;
                ScrollLinesUp();
                RebuildAllLinesAfterScroll();
            }
        }

        /// <summary>
        /// Rebuilds all lines after a scroll operation.
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
        /// Determines if the highlight is in the second line at approximately one-third position.
        /// </summary>
        /// <param name="secondLine">The second line to check.</param>
        /// <returns>True if the highlight is at one-third position in the second line; otherwise, false.</returns>
        private bool IsHighlightInSecondLineAtOneThird(KaraokeLine secondLine)
        {
            if (_currentGlobalSyllableIndex < secondLine.FirstSyllableGlobalIndex ||
                _currentGlobalSyllableIndex > secondLine.LastSyllableGlobalIndex)
                return false;

            var totalSyllablesInSecondLine = secondLine.Syllables.Count;
            if (totalSyllablesInSecondLine == 0)
                return false;

            var currentSyllableInSecondLine = secondLine.Syllables.FirstOrDefault(s =>
                s.GlobalIndex == _currentGlobalSyllableIndex);

            if (currentSyllableInSecondLine == null)
                return false;

            var syllableIndexInLine = secondLine.Syllables.IndexOf(currentSyllableInSecondLine);

            return syllableIndexInLine >= totalSyllablesInSecondLine / 3.0;
        }

        /// <summary>
        /// Scrolls all lines up by removing the first line and adjusting positions.
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
