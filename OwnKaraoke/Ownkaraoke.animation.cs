using Avalonia.Controls;
using Avalonia.Threading;

namespace OwnKaraoke
{
    public partial class OwnKaraokeDisplay : Control
    {
        #region Animation Control

        /// <summary>
        /// Starts the karaoke animation timer.
        /// </summary>
        private void StartAnimation()
        {
            if (_animationSubscription != null || _itemsSourceInternal.Count == 0 ||
                _currentGlobalSyllableIndex >= _itemsSourceInternal.Count)
                return;

            _lastFrameTime = TimeSpan.Zero;

            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            timer.Tick += OnTimerTick;
            timer.Start();

            _animationSubscription = new TimerDisposable(timer);
        }

        /// <summary>
        /// Handles timer tick events for animation frames.
        /// </summary>
        /// <param name="sender">The timer object.</param>
        /// <param name="e">The event arguments.</param>
        private void OnTimerTick(object? sender, EventArgs e) =>
            OnFrame(TimeSpan.FromSeconds(Environment.TickCount / 1000.0));

        /// <summary>
        /// Stops the animation timer.
        /// </summary>
        private void StopAnimation()
        {
            _animationSubscription?.Dispose();
            _animationSubscription = null;
        }

        /// <summary>
        /// Processes a single animation frame.
        /// </summary>
        /// <param name="totalTime">The total elapsed time since animation started.</param>
        private void OnFrame(TimeSpan totalTime)
        {
            if (!IsEffectivelyVisible)
            {
                StopAnimation();
                return;
            }

            if (_lastFrameTime == TimeSpan.Zero)
            {
                _lastFrameTime = totalTime;
                return;
            }

            var deltaTime = totalTime - _lastFrameTime;
            _lastFrameTime = totalTime;

            if (_itemsSourceInternal.Count == 0 || _currentGlobalSyllableIndex >= _itemsSourceInternal.Count)
            {
                StopAnimation();
                InvalidateVisual();
                return;
            }

            UpdateAnimationLogic(deltaTime.TotalMilliseconds);
            UpdateLineAnimations(deltaTime.TotalMilliseconds);
            InvalidateVisual();
        }

        /// <summary>
        /// Updates line position and opacity animations.
        /// </summary>
        /// <param name="elapsedMs">The elapsed time in milliseconds since the last frame.</param>
        private void UpdateLineAnimations(double elapsedMs)
        {
            _isAnimatingLines = false;

            foreach (var line in _displayLines)
            {
                var hasPositionAnimation = false;
                var hasOpacityAnimation = false;

                // Position animation
                var positionDistance = Math.Abs(line.CurrentY - line.TargetY);
                if (positionDistance <= 0.5)
                {
                    line.CurrentY = line.TargetY;
                }
                else
                {
                    var direction = line.TargetY > line.CurrentY ? 1 : -1;
                    var moveAmount = LINE_ANIMATION_SPEED * elapsedMs;
                    var easingFactor = Math.Min(1.0, positionDistance / 20.0);
                    moveAmount *= (0.3 + 0.7 * easingFactor);

                    line.CurrentY += direction * moveAmount;

                    if ((direction > 0 && line.CurrentY > line.TargetY) ||
                        (direction < 0 && line.CurrentY < line.TargetY))
                    {
                        line.CurrentY = line.TargetY;
                    }
                    else
                    {
                        hasPositionAnimation = true;
                    }
                }

                // Opacity animation
                var opacityDistance = Math.Abs(line.Opacity - line.TargetOpacity);
                if (opacityDistance <= 0.01)
                {
                    line.Opacity = line.TargetOpacity;
                }
                else
                {
                    var opacityDirection = line.TargetOpacity > line.Opacity ? 1 : -1;
                    var opacityChange = OPACITY_ANIMATION_SPEED * elapsedMs;

                    line.Opacity += opacityDirection * opacityChange;
                    line.Opacity = Math.Clamp(line.Opacity, 0.0, 1.0);

                    if ((opacityDirection > 0 && line.Opacity >= line.TargetOpacity) ||
                        (opacityDirection < 0 && line.Opacity <= line.TargetOpacity))
                    {
                        line.Opacity = line.TargetOpacity;
                    }
                    else
                    {
                        hasOpacityAnimation = true;
                    }
                }

                if (hasPositionAnimation || hasOpacityAnimation)
                    _isAnimatingLines = true;
            }
        }

        /// <summary>
        /// Updates the karaoke animation with simplified logic.
        /// </summary>
        /// <param name="elapsedMs">Elapsed time since last frame.</param>
        private void UpdateAnimationLogic(double elapsedMs)
        {
            if (_currentGlobalSyllableIndex >= _itemsSourceInternal.Count)
            {
                if (Status != KaraokeStatus.Finished)
                {
                    Status = KaraokeStatus.Finished;
                    OriginalPosition = OriginalDuration;
                    Position = Duration;
                }
                return;
            }

            var syllableAdvanced = false;

            _timeSinceLastSeekMs += elapsedMs;
            _originalElapsedTimeMs = _lastSeekPositionMs + _timeSinceLastSeekMs;

            OriginalPosition = _originalElapsedTimeMs;
            Position = ApplyTempoToTime(_originalElapsedTimeMs);

            var tempoAdjustedElapsedMs = ApplyTempoToElapsedTime(elapsedMs);
            var totalElapsedTime = _timeElapsedInCurrentSyllableMs + tempoAdjustedElapsedMs;

            while (_currentGlobalSyllableIndex < _itemsSourceInternal.Count)
            {
                var currentSyllable = _itemsSourceInternal[_currentGlobalSyllableIndex];
                var tempoAdjustedStartTime = ApplyTempoToTime(currentSyllable.StartTimeMs);

                if (totalElapsedTime < tempoAdjustedStartTime)
                {
                    break;
                }

                // Check if we should advance to next syllable
                var tempoAdjustedDuration = ApplyTempoToTime(CalculateSyllableDuration(_currentGlobalSyllableIndex));
                var timeIntoSyllable = totalElapsedTime - tempoAdjustedStartTime;

                if (timeIntoSyllable >= tempoAdjustedDuration)
                {
                    _currentGlobalSyllableIndex++;
                    syllableAdvanced = true;

                    if (_currentGlobalSyllableIndex >= _itemsSourceInternal.Count)
                    {
                        Status = KaraokeStatus.Finished;
                        OriginalPosition = OriginalDuration;
                        Position = Duration;
                        StopAnimation();
                        return;
                    }
                }
                else
                {
                    break;
                }
            }

            _timeElapsedInCurrentSyllableMs = totalElapsedTime;

            if (syllableAdvanced && _displayLines.Count > 0)
            {
                CheckForLineScroll();
            }
        }

        #endregion
    }
}
