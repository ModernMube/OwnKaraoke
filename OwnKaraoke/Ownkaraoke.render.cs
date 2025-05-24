using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;

namespace OwnKaraoke
{
    public partial class OwnKaraokeDisplay : Control
    {
        #region Rendering

        /// <summary>
        /// Renders the karaoke display.
        /// </summary>
        /// <param name="context">The drawing context.</param>
        public override void Render(DrawingContext context)
        {
            base.Render(context);

            if (_itemsSourceInternal.Count == 0 || _displayLines.Count == 0)
                return;

            foreach (var line in _displayLines)
                RenderLine(context, line);
        }

        /// <summary>
        /// Renders a single line of karaoke text.
        /// </summary>
        /// <param name="context">The drawing context.</param>
        /// <param name="line">The line to render.</param>
        private void RenderLine(DrawingContext context, KaraokeLine line)
        {
            if (line.Syllables.Count == 0)
                return;

            var lineX = CalculateLineX(line.LineWidth);

            if (line.Opacity < 1.0)
            {
                using (context.PushOpacity(line.Opacity))
                {
                    foreach (var syllable in line.Syllables)
                        RenderSyllable(context, syllable, lineX, line.CurrentY);
                }
            }
            else
            {
                foreach (var syllable in line.Syllables)
                    RenderSyllable(context, syllable, lineX, line.CurrentY);
            }
        }

        /// <summary>
        /// Calculates the X position for a line based on text alignment.
        /// </summary>
        /// <param name="lineWidth">The width of the line.</param>
        /// <returns>The X position for the line.</returns>
        private double CalculateLineX(double lineWidth)
        {
            var x = TextAlignment switch
            {
                TextAlignment.Center => (Bounds.Width - lineWidth) / 2,
                TextAlignment.Right => Bounds.Width - lineWidth,
                _ => 0
            };
            return Math.Max(0, x);
        }

        /// <summary>
        /// Renders a single syllable with appropriate highlighting based on its state.
        /// </summary>
        /// <param name="context">The drawing context.</param>
        /// <param name="syllable">The syllable to render.</param>
        /// <param name="lineX">The X position of the line.</param>
        /// <param name="lineY">The Y position of the line.</param>
        private void RenderSyllable(DrawingContext context, SyllableMetrics syllable, double lineX, double lineY)
        {
            if (string.IsNullOrEmpty(syllable.OriginalElement.Text) && syllable.Width == 0)
                return;

            var origin = new Point(lineX + syllable.XOffsetInLine, lineY);

            if (syllable.GlobalIndex < _currentGlobalSyllableIndex)
            {
                RenderAlreadySungSyllable(context, syllable, origin);
            }
            else if (syllable.GlobalIndex == _currentGlobalSyllableIndex)
            {
                RenderCurrentSyllable(context, syllable, origin);
            }
            else
            {
                RenderFutureSyllable(context, syllable, origin);
            }
        }

        /// <summary>
        /// Renders a syllable that has already been sung.
        /// </summary>
        /// <param name="context">The drawing context.</param>
        /// <param name="syllable">The syllable to render.</param>
        /// <param name="origin">The origin point for rendering.</param>
        private void RenderAlreadySungSyllable(DrawingContext context, SyllableMetrics syllable, Point origin)
        {
            var text = GetOrCreateFormattedText(syllable.OriginalElement.Text,
                AlreadySungBrush ?? Foreground ?? Brushes.White);
            context.DrawText(text, origin);
        }

        /// <summary>
        /// Renders the currently highlighted syllable with progressive highlighting.
        /// </summary>
        /// <param name="context">The drawing context.</param>
        /// <param name="syllable">The syllable to render.</param>
        /// <param name="origin">The origin point for rendering.</param>
        private void RenderCurrentSyllable(DrawingContext context, SyllableMetrics syllable, Point origin)
        {
            var normalText = GetOrCreateFormattedText(syllable.OriginalElement.Text, Foreground ?? Brushes.White);
            context.DrawText(normalText, origin);

            var highlightRatio = CalculateHighlightRatio(syllable);
            var highlightWidth = syllable.Width * highlightRatio;

            if (highlightWidth > 0 && HighlightBrush != null)
            {
                var highlightText = GetOrCreateFormattedText(syllable.OriginalElement.Text, HighlightBrush);
                using (context.PushClip(new Rect(origin.X, origin.Y, highlightWidth, syllable.Height)))
                {
                    context.DrawText(highlightText, origin);
                }
            }
        }

        /// <summary>
        /// Renders a syllable that will be sung in the future.
        /// </summary>
        /// <param name="context">The drawing context.</param>
        /// <param name="syllable">The syllable to render.</param>
        /// <param name="origin">The origin point for rendering.</param>
        private void RenderFutureSyllable(DrawingContext context, SyllableMetrics syllable, Point origin)
        {
            var text = GetOrCreateFormattedText(syllable.OriginalElement.Text, Foreground ?? Brushes.White);
            context.DrawText(text, origin);
        }

        /// <summary>
        /// Calculates the highlight ratio for the current syllable based on timing.
        /// </summary>
        /// <param name="syllable">The syllable to calculate the ratio for.</param>
        /// <returns>A value between 0.0 and 1.0 representing the highlight progress.</returns>
        private double CalculateHighlightRatio(SyllableMetrics syllable)
        {
            var currentElement = syllable.OriginalElement;

            if (_timeElapsedInCurrentSyllableMs < currentElement.StartTimeMs)
                return 0.0;

            if (syllable.GlobalIndex == _currentGlobalSyllableIndex)
            {
                var syllableDuration = CalculateSyllableDuration(syllable.GlobalIndex);
                var timeIntoSyllable = _timeElapsedInCurrentSyllableMs - currentElement.StartTimeMs;

                if (syllableDuration <= 0)
                    return 1.0;

                return Math.Clamp(timeIntoSyllable / syllableDuration, 0.0, 1.0);
            }

            if (syllable.GlobalIndex < _currentGlobalSyllableIndex)
                return 1.0;

            return 0.0;
        }

        #endregion
    }
}
