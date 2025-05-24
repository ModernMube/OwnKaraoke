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
        #region Public Control Methods

        /// <summary>
        /// Starts the karaoke display from the beginning.
        /// </summary>
        public void Start()
        {
            _itemsSourceInternal.Clear();
            if (ItemsSource != null)
                _itemsSourceInternal.AddRange(ItemsSource);

            ResetAndBuildLines();
        }

        /// <summary>
        /// Stops the karaoke animation and resets its progress to the beginning.
        /// The display will show the initial set of lines, static.
        /// </summary>
        public void Stop()
        {
            StopAnimation();

            _currentGlobalSyllableIndex = 0;
            _timeElapsedInCurrentSyllableMs = 0;
            _firstSyllableIndexForLineBuilding = 0;
            _isAnimatingLines = false;

            if (IsAttachedToVisualTree)
                BuildLines();
            else
                InvalidateVisual();
        }

        /// <summary>
        /// Pauses the karaoke animation at the current position.
        /// The current highlighting state is preserved.
        /// </summary>
        public void Pause() => StopAnimation();

        /// <summary>
        /// Resumes the karaoke animation from the paused state.
        /// </summary>
        public void Resume()
        {
            if (IsAttachedToVisualTree && _itemsSourceInternal.Count > 0 &&
                _currentGlobalSyllableIndex < _itemsSourceInternal.Count)
            {
                StartAnimation();
            }
        }

        #endregion
    }
}
