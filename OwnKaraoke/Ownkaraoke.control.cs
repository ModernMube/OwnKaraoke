using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwnKaraoke
{
    public partial class OwnKaraokeDisplay : Control
    {
        #region Modified Public Control Methods

        /// <summary>
        /// Starts the karaoke from the beginning.
        /// </summary>
        public void Start()
        {
            _itemsSourceInternal.Clear();
            if (ItemsSource != null)
                _itemsSourceInternal.AddRange(ItemsSource);

            CalculateDuration();

            if (_itemsSourceInternal.Count > 0)
            {
                // Reset to beginning
                ResetToPosition(0, 0);
                SetupDisplayFromSyllable(0);
                Status = KaraokeStatus.Playing;

                if (IsAttachedToVisualTree)
                {
                    BuildLines();
                    StartAnimation();
                }
            }
            else
            {
                Status = KaraokeStatus.Idle;
            }
        }

        /// <summary>
        /// Stops and resets to beginning.
        /// </summary>
        public void Stop()
        {
            StopAnimation();

            if (_itemsSourceInternal.Count > 0)
            {
                ResetToPosition(0, 0);
                SetupDisplayFromSyllable(0);
            }

            Status = KaraokeStatus.Idle;

            if (IsAttachedToVisualTree)
                BuildLines();
            else
                InvalidateVisual();
        }

        /// <summary>
        /// Pauses at current position.
        /// </summary>
        public void Pause()
        {
            if (Status == KaraokeStatus.Playing)
            {
                StopAnimation();
                Status = KaraokeStatus.Paused;
            }
        }

        /// <summary>
        /// Resumes from paused state.
        /// </summary>
        public void Resume()
        {
            if (Status == KaraokeStatus.Paused && IsAttachedToVisualTree &&
                _itemsSourceInternal.Count > 0 && _currentGlobalSyllableIndex < _itemsSourceInternal.Count)
            {
                Status = KaraokeStatus.Playing;
                StartAnimation();
            }
        }

        #endregion

        #region Simple Seek Implementation

        /// <summary>
        /// Seeks to the specified position in milliseconds using a simple reset approach.
        /// </summary>
        /// <param name="positionMs">The target position in milliseconds.</param>
        public void Seek(double positionMs)
        {
            if (_itemsSourceInternal.Count == 0)
                return;

            positionMs = Math.Max(0, Math.Min(positionMs, OriginalDuration));

            var wasPlaying = Status == KaraokeStatus.Playing;

            StopAnimation();

            var targetSyllableIndex = FindSyllableAtPosition(positionMs);

            ResetToPosition(positionMs, targetSyllableIndex);

            SetupDisplayFromSyllable(targetSyllableIndex);

            if (IsAttachedToVisualTree)
            {
                BuildLines();
                InvalidateVisual();
            }

            if (wasPlaying && targetSyllableIndex < _itemsSourceInternal.Count)
            {
                Status = KaraokeStatus.Playing;
                StartAnimation();
            }
            else if (targetSyllableIndex >= _itemsSourceInternal.Count)
            {
                Status = KaraokeStatus.Finished;
            }
            else
            {
                Status = KaraokeStatus.Paused;
            }
        }

        /// <summary>
        /// Finds the syllable at the specified position.
        /// </summary>
        /// <param name="positionMs">Position in milliseconds.</param>
        /// <returns>Syllable index.</returns>
        private int FindSyllableAtPosition(double positionMs)
        {
            for (int i = 0; i < _itemsSourceInternal.Count; i++)
            {
                if (_itemsSourceInternal[i].StartTimeMs > positionMs)
                {
                    return Math.Max(0, i - 1);
                }
            }
            return _itemsSourceInternal.Count - 1;
        }

        /// <summary>
        /// Resets all timing variables to the specified position.
        /// </summary>
        /// <param name="positionMs">The position to reset to.</param>
        /// <param name="syllableIndex">The syllable index at this position.</param>
        private void ResetToPosition(double positionMs, int syllableIndex)
        {
            _currentGlobalSyllableIndex = syllableIndex;
            _originalElapsedTimeMs = positionMs;
            _lastSeekPositionMs = positionMs;
            _timeSinceLastSeekMs = 0;
            _timeElapsedInCurrentSyllableMs = ApplyTempoToTime(positionMs);
            _isAnimatingLines = false;

            OriginalPosition = positionMs;
            Position = ApplyTempoToTime(positionMs);
        }

        /// <summary>
        /// Sets up the display to start from the specified syllable.
        /// The target syllable's line will be the first line displayed.
        /// </summary>
        /// <param name="syllableIndex">The syllable to start displaying from.</param>
        private void SetupDisplayFromSyllable(int syllableIndex)
        {
            if (syllableIndex >= _itemsSourceInternal.Count)
            {
                _firstSyllableIndexForLineBuilding = Math.Max(0, _itemsSourceInternal.Count - 1);
                return;
            }

            var lineStartIndex = FindLineStartForSyllable(syllableIndex);
            _firstSyllableIndexForLineBuilding = lineStartIndex;
        }

        /// <summary>
        /// Finds the start index of the line that should contain the specified syllable.
        /// This ensures the target syllable appears in the first displayed line.
        /// </summary>
        /// <param name="targetSyllableIndex">The syllable that should be visible.</param>
        /// <returns>The index where line building should start.</returns>
        private int FindLineStartForSyllable(int targetSyllableIndex)
        {
            if (targetSyllableIndex <= 0 || Bounds.Width <= 0)
                return 0;

            var estimatedSyllablesPerLine = EstimateSyllablesPerLine();

            var startIndex = Math.Max(0, targetSyllableIndex - (estimatedSyllablesPerLine / 2));

            if (!WillSyllableBeVisible(startIndex, targetSyllableIndex))
            {
                startIndex = targetSyllableIndex;
            }

            return startIndex;
        }

        /// <summary>
        /// Estimates how many syllables typically fit in one line.
        /// </summary>
        /// <returns>Estimated syllables per line.</returns>
        private int EstimateSyllablesPerLine()
        {
            if (Bounds.Width <= 0 || FontSize <= 0)
                return 10; // Reasonable default

            var avgCharWidth = FontSize * 0.6; // Rough estimate
            var avgSyllableLength = 4; // Average syllable length in characters
            var avgSyllableWidth = avgCharWidth * avgSyllableLength;

            var syllablesPerLine = (int)(Bounds.Width / avgSyllableWidth);
            return Math.Max(5, Math.Min(syllablesPerLine, 20)); // Reasonable bounds
        }

        /// <summary>
        /// Checks if a syllable would be visible when building lines from a start index.
        /// </summary>
        /// <param name="startIndex">Line building start index.</param>
        /// <param name="targetSyllableIndex">Syllable to check visibility for.</param>
        /// <returns>True if the syllable would be visible.</returns>
        private bool WillSyllableBeVisible(int startIndex, int targetSyllableIndex)
        {
            var maxSyllablesToCheck = VisibleLinesCount * EstimateSyllablesPerLine();
            var endIndex = Math.Min(_itemsSourceInternal.Count, startIndex + maxSyllablesToCheck);

            return targetSyllableIndex >= startIndex && targetSyllableIndex < endIndex;
        }

        /// <summary>
        /// Calculates the total duration based on the elements.
        /// </summary>
        private void CalculateDuration()
        {
            if (_itemsSourceInternal.Count == 0)
            {
                Duration = 0;
                OriginalDuration = 0;
                return;
            }

            var lastElement = _itemsSourceInternal.LastOrDefault();
            if (lastElement != null)
            {
                var lastSyllableDuration = CalculateLastSyllableDuration();
                var originalDuration = lastElement.StartTimeMs + lastSyllableDuration;
                OriginalDuration = originalDuration;

                Duration = ApplyTempoToTime(originalDuration);
            }
        }

        /// <summary>
        /// Calculates appropriate duration for the last syllable.
        /// </summary>
        /// <returns>Duration in milliseconds.</returns>
        private double CalculateLastSyllableDuration()
        {
            if (_itemsSourceInternal.Count < 2)
                return 2000; // 2 seconds default

            var lastElement = _itemsSourceInternal[^1];
            var secondLastElement = _itemsSourceInternal[^2];

            var timeDifference = lastElement.StartTimeMs - secondLastElement.StartTimeMs;
            return Math.Max(1000, Math.Min(timeDifference, 3000)); // Between 1-3 seconds
        }

        /// <summary>
        /// Updates the Duration property when Tempo changes.
        /// </summary>
        private void UpdateDurationForTempo()
        {
            if (OriginalDuration > 0)
            {
                Duration = ApplyTempoToTime(OriginalDuration);
            }
        }

        #endregion
    }
}
