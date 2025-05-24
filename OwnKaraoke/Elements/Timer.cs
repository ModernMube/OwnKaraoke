using Avalonia.Threading;
using System;

namespace OwnKaraoke
{
    /// <summary>
    /// Helper class for managing timer disposal to prevent memory leaks.
    /// </summary>
    internal sealed class TimerDisposable : IDisposable
    {
        private readonly DispatcherTimer _timer;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimerDisposable"/> class.
        /// </summary>
        /// <param name="timer">The timer to manage.</param>
        public TimerDisposable(DispatcherTimer timer) => _timer = timer;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _timer?.Stop();
                _disposed = true;
            }
        }
    }
}
