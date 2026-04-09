using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var calculatedHeight = _displayLines.Sum(l => l.LineHeight);
            var calculatedWidth = _displayLines.Count > 0 ? _displayLines.Max(l => l.LineWidth) : 0;

            if (calculatedHeight == 0 && VisibleLinesCount > 0 && _itemsSourceInternal.Count > 0)
            {
                calculatedHeight = VisibleLinesCount * (FontSize * 1.2);
                calculatedWidth = 0;
            }

            return new Size(calculatedWidth, calculatedHeight);
        }

        #endregion
    }
}
