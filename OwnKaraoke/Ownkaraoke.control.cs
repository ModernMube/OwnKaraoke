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
        /// FIX: With proper position reset implementation.
        /// </summary>
        public void Start()
        {
            _itemsSourceInternal.Clear();
            if (ItemsSource != null)
                _itemsSourceInternal.AddRange(ItemsSource);

            CalculateDuration();

            if (_itemsSourceInternal.Count > 0)
            {
                // FIX: Proper method call
                ResetToPositionWithTempo(0, 0);
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
        /// Stops and resets to the beginning.
        /// FIX: With proper position reset implementation.
        /// </summary>
        public void Stop()
        {
            StopAnimation();

            if (_itemsSourceInternal.Count > 0)
            {
                // FIX: Proper method call
                ResetToPositionWithTempo(0, 0);
                SetupDisplayFromSyllable(0);
            }

            Status = KaraokeStatus.Idle;

            if (IsAttachedToVisualTree)
                BuildLines();
            else
                InvalidateVisual();
        }

        /// <summary>
        /// Pauses at the current position.
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

        #region Improved Seek Implementation with Tempo Support

        /// <summary>
        /// Seeks to the specified position in milliseconds.
        /// Improved version: with perfect tempo handling and scrolling fixes.
        /// </summary>
        /// <param name="positionMs">The target position in milliseconds (original time, without tempo).</param>
        public void Seek(double positionMs)
        {
            if (_itemsSourceInternal.Count == 0)
                return;

            positionMs = Math.Max(0, Math.Min(positionMs, OriginalDuration));

            var wasPlaying = Status == KaraokeStatus.Playing;

            StopAnimation();

            var targetSyllableIndex = FindSyllableAtPosition(positionMs);

            // Tempo-aware position reset
            ResetToPositionWithTempo(positionMs, targetSyllableIndex);

            SetupDisplayFromSyllable(targetSyllableIndex);

            if (IsAttachedToVisualTree)
            {
                BuildLines();
                CheckScrollingAfterSeek(targetSyllableIndex);
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
        /// Resets all timing variables to the specified position in a tempo-aware way.
        /// This method replaces the missing ResetToPosition() method.
        /// </summary>
        /// <param name="originalPositionMs">The original position in milliseconds (without tempo).</param>
        /// <param name="syllableIndex">The syllable index at this position.</param>
        private void ResetToPositionWithTempo(double originalPositionMs, int syllableIndex)
        {
            _currentGlobalSyllableIndex = syllableIndex;

            // Original time variables (without tempo)
            _originalElapsedTimeMs = originalPositionMs;
            _lastSeekPositionMs = originalPositionMs;
            _timeSinceLastSeekMs = 0;

            // Tempo-modified time variables
            var tempoAdjustedPosition = ApplyTempoToTime(originalPositionMs);
            _timeElapsedInCurrentSyllableMs = tempoAdjustedPosition;

            _isAnimatingLines = false;

            // Update properties
            OriginalPosition = originalPositionMs;
            Position = tempoAdjustedPosition;
        }

        /// <summary>
        /// Alternative method name - compatibility with original code.
        /// Simply delegates to ResetToPositionWithTempo.
        /// </summary>
        /// <param name="positionMs">The position in milliseconds.</param>
        /// <param name="syllableIndex">The syllable index.</param>
        private void ResetToPosition(double positionMs, int syllableIndex)
        {
            ResetToPositionWithTempo(positionMs, syllableIndex);
        }

        /// <summary>
        /// Finds the syllable at the specified original position.
        /// </summary>
        /// <param name="originalPositionMs">Original position in milliseconds (without tempo).</param>
        /// <returns>Syllable index.</returns>
        private int FindSyllableAtPosition(double originalPositionMs)
        {
            for (int i = 0; i < _itemsSourceInternal.Count; i++)
            {
                if (_itemsSourceInternal[i].StartTimeMs > originalPositionMs)
                {
                    return Math.Max(0, i - 1);
                }
            }
            return Math.Min(_itemsSourceInternal.Count - 1, Math.Max(0, _itemsSourceInternal.Count - 1));
        }

        /// <summary>
        /// Sets up the display to start from the specified syllable.
        /// </summary>
        /// <param name="syllableIndex">The syllable from which to start the display.</param>
        private void SetupDisplayFromSyllable(int syllableIndex)
        {
            if (syllableIndex >= _itemsSourceInternal.Count)
            {
                _firstSyllableIndexForLineBuilding = Math.Max(0, _itemsSourceInternal.Count - 1);
                return;
            }

            var lineStartIndex = CalculateOptimalLineStartForSyllable(syllableIndex);
            _firstSyllableIndexForLineBuilding = lineStartIndex;
        }

        /// <summary>
        /// Calculates the optimal line starting index for a target syllable.
        /// </summary>
        /// <param name="targetSyllableIndex">The syllable that needs to be visible.</param>
        /// <returns>The index where line building should start.</returns>
        private int CalculateOptimalLineStartForSyllable(int targetSyllableIndex)
        {
            if (targetSyllableIndex <= 0 || Bounds.Width <= 0)
                return 0;

            var estimatedSyllablesPerLine = EstimateSyllablesPerLine();
            var totalVisibleSyllables = estimatedSyllablesPerLine * VisibleLinesCount;

            // The target syllable should be in the first third of the first line
            var targetPositionInDisplay = estimatedSyllablesPerLine / 3;
            var optimalStart = Math.Max(0, targetSyllableIndex - targetPositionInDisplay);

            // Don't go too far from the end of the content
            var maxPossibleStart = Math.Max(0, _itemsSourceInternal.Count - totalVisibleSyllables);
            optimalStart = Math.Min(optimalStart, maxPossibleStart);

            // Check if the target syllable will be visible
            if (!WillSyllableBeVisible(optimalStart, targetSyllableIndex))
            {
                optimalStart = Math.Max(0, targetSyllableIndex - estimatedSyllablesPerLine + 1);
            }

            return optimalStart;
        }

        /// <summary>
        /// Provides an estimate of how many syllables generally fit in a line.
        /// </summary>
        /// <returns>Estimated number of syllables per line.</returns>
        private int EstimateSyllablesPerLine()
        {
            if (Bounds.Width <= 0 || FontSize <= 0)
                return 10; // Reasonable default

            var avgCharWidth = FontSize * 0.6; // Rough estimate
            var avgSyllableLength = 4; // Average syllable length in characters
            var avgSyllableWidth = avgCharWidth * avgSyllableLength;

            var syllablesPerLine = (int)(Bounds.Width / avgSyllableWidth);
            return Math.Max(5, Math.Min(syllablesPerLine, 20)); // Reasonable limits
        }

        /// <summary>
        /// Checks if a syllable will be visible if we build lines from a starting index.
        /// </summary>
        /// <param name="startIndex">Line building start index.</param>
        /// <param name="targetSyllableIndex">The syllable to check for visibility.</param>
        /// <returns>True if the syllable will be visible.</returns>
        private bool WillSyllableBeVisible(int startIndex, int targetSyllableIndex)
        {
            var maxSyllablesToCheck = VisibleLinesCount * EstimateSyllablesPerLine();
            var endIndex = Math.Min(_itemsSourceInternal.Count, startIndex + maxSyllablesToCheck);

            return targetSyllableIndex >= startIndex && targetSyllableIndex < endIndex;
        }

        /// <summary>
        /// Calculates the total duration based on the items.
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
        /// Calculates the appropriate duration for the last syllable.
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