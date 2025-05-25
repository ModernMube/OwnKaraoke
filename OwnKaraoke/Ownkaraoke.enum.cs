using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwnKaraoke
{
    /// <summary>
    /// Represents the current status of the karaoke control.
    /// </summary>
    public enum KaraokeStatus
    {
        /// <summary>
        /// The control is in idle state (no data loaded or stopped).
        /// </summary>
        Idle,

        /// <summary>
        /// The karaoke is currently playing.
        /// </summary>
        Playing,

        /// <summary>
        /// The karaoke is paused.
        /// </summary>
        Paused,

        /// <summary>
        /// The karaoke has finished playing.
        /// </summary>
        Finished
    }
}
