using Avalonia;
using Avalonia.Controls;

namespace OwnKaraoke
{
    public partial class OwnKaraokeDisplay : Control
    {
        #region Layout

        /// <summary>
        /// Measures the desired size of the karaoke display.
        /// </summary>
        /// <param name="availableSize">The available size.</param>
        /// <returns>The desired size.</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            double calculatedHeight = 0;
            double calculatedWidth  = 0;
            foreach (var line in _displayLines)
            {
                calculatedHeight += line.LineHeight;
                if (line.LineWidth > calculatedWidth)
                    calculatedWidth = line.LineWidth;
            }

            if (calculatedHeight == 0 && VisibleLinesCount > 0 && _itemsSourceInternal.Count > 0)
            {
                calculatedHeight = VisibleLinesCount * (FontSize * 1.2);
                calculatedWidth  = 0;
            }

            return new Size(calculatedWidth, calculatedHeight);
        }

        #endregion
    }
}
