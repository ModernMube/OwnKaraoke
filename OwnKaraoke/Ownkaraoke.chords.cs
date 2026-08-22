using Avalonia.Controls;
using Avalonia.Media;
using System.Collections.Generic;
using System.Globalization;

namespace OwnKaraoke
{
    public partial class OwnKaraokeDisplay : Control
    {
        #region Chord row

        /// <summary>
        /// Gap kept between two chord labels that would otherwise sit on top of each other.
        /// </summary>
        private const double CHORD_GAP_PX = 6.0;

        private readonly List<TimedChordElement> _chordsInternal = new();

        /// <summary>
        /// Chord labels are the same in every line, so one cache serves them all.
        /// </summary>
        private readonly Dictionary<string, FormattedText> _chordTextCache = new();

        /// <summary>
        /// Height of the chord row, identical for every line so the lyrics keep an even
        /// rhythm. Zero while there is nothing to show.
        /// </summary>
        private double _chordRowHeight;

        private static readonly System.Comparison<TimedChordElement> ChordByTime =
            (a, b) => a.StartTimeMs.CompareTo(b.StartTimeMs);

        /// <summary>
        /// Handles changes to the ChordSource property.
        /// </summary>
        private void HandleChordSourceChanged(IEnumerable<TimedChordElement>? newValue)
        {
            _chordsInternal.Clear();
            if (newValue != null)
                _chordsInternal.AddRange(newValue);

            _chordsInternal.Sort(ChordByTime);
            _chordTextCache.Clear();

            if (IsAttachedToVisualTree)
                BuildLines();
            else
                InvalidateVisual();
        }

        /// <summary>
        /// Colour or size changed under us.
        /// </summary>
        private void HandleChordStyleChanged()
        {
            _chordTextCache.Clear();

            if (IsAttachedToVisualTree)
                BuildLines();
        }

        /// <summary>
        /// Called once per line rebuild — the row is only as tall as the font asks for.
        /// </summary>
        private void UpdateChordRowHeight() =>
            _chordRowHeight = _chordsInternal.Count > 0 ? FontSize * ChordFontScale * 1.25 : 0.0;

        /// <summary>
        /// Hangs every chord of this line's time span over the syllable being sung when it
        /// starts, at the beginning, the middle or the end of that syllable depending on how
        /// far into it the chord falls. Labels that would collide are pushed right instead of
        /// drawn on top of each other.
        /// </summary>
        private void AssignChords(KaraokeLine line)
        {
            line.Chords.Clear();
            line.ChordRowHeight = _chordRowHeight;

            if (_chordsInternal.Count == 0 || line.Syllables.Count == 0)
                return;

            var lineStart = line.Syllables[0].OriginalElement.StartTimeMs;
            var lineEnd = GetLineEndTime(line);
            var minX = double.NegativeInfinity;

            var first = FindFirstChordFrom(lineStart);

            // A chord held over from the previous line still has to be readable here,
            // otherwise the singer sees nothing until the next change.
            if (first > 0 && _chordsInternal[first - 1].StartTimeMs < lineStart)
            {
                var carried = GetOrCreateChordText(_chordsInternal[first - 1].Chord);
                line.Chords.Add(new ChordLabel(0.0, carried));
                minX = carried.Width + CHORD_GAP_PX;
            }

            for (var i = first; i < _chordsInternal.Count; i++)
            {
                var chord = _chordsInternal[i];
                if (chord.StartTimeMs >= lineEnd)
                    break;

                var syllable = FindSyllableAt(line, chord.StartTimeMs);
                if (syllable == null)
                    continue;

                var text = GetOrCreateChordText(chord.Chord);
                var x = GetChordX(syllable, chord.StartTimeMs);

                // An end-of-syllable anchor on the last word would hang the label off the line.
                var rightmost = line.LineWidth - text.Width;
                if (x > rightmost) x = rightmost > 0.0 ? rightmost : 0.0;

                if (x < minX) x = minX;

                line.Chords.Add(new ChordLabel(x, text));
                minX = x + text.Width + CHORD_GAP_PX;
            }
        }

        /// <summary>
        /// Where the line stops: the next element's time, which for a full line is the line
        /// break marker holding the following line's start.
        /// </summary>
        private double GetLineEndTime(KaraokeLine line)
        {
            var next = line.LastSyllableGlobalIndex + 1;
            return next < _itemsSourceInternal.Count
                ? _itemsSourceInternal[next].StartTimeMs
                : double.MaxValue;
        }

        /// <summary>
        /// Index of the first chord at or after the given time.
        /// </summary>
        private int FindFirstChordFrom(double timeMs)
        {
            int lo = 0, hi = _chordsInternal.Count - 1, found = _chordsInternal.Count;

            while (lo <= hi)
            {
                var mid = (lo + hi) >> 1;
                if (_chordsInternal[mid].StartTimeMs >= timeMs)
                {
                    found = mid;
                    hi = mid - 1;
                }
                else
                {
                    lo = mid + 1;
                }
            }

            return found;
        }

        /// <summary>
        /// The syllable already started at that moment, or the first one if the chord comes
        /// in slightly ahead of the line.
        /// </summary>
        private static SyllableMetrics? FindSyllableAt(KaraokeLine line, double timeMs)
        {
            var syllables = line.Syllables;

            for (var i = syllables.Count - 1; i >= 0; i--)
            {
                if (syllables[i].OriginalElement.StartTimeMs <= timeMs)
                    return syllables[i];
            }

            return syllables.Count > 0 ? syllables[0] : null;
        }

        /// <summary>
        /// Where the chord goes above the syllable. The syllable lasts until the next element
        /// starts; that span is split into three, and the chord is drawn at whichever of the
        /// three anchors — beginning, middle or end of the syllable — its time is closest to.
        /// </summary>
        private double GetChordX(SyllableMetrics syllable, double timeMs)
        {
            var start = syllable.OriginalElement.StartTimeMs;
            var third = (GetSyllableEndTime(syllable) - start) / 3.0;

            // Nothing to divide when the timings give the syllable no length of its own.
            if (third <= 0.0)
                return syllable.XOffsetInLine;

            var elapsed = timeMs - start;

            if (elapsed < third * 0.5)
                return syllable.XOffsetInLine;

            if (elapsed < third * 1.5)
                return syllable.XOffsetInLine + syllable.Width * 0.5;

            return syllable.XOffsetInLine + syllable.Width;
        }

        /// <summary>
        /// When the syllable stops sounding: the moment the next element starts. The last
        /// element of the source has no successor, so it is treated as having no length.
        /// </summary>
        private double GetSyllableEndTime(SyllableMetrics syllable)
        {
            var next = syllable.GlobalIndex + 1;
            return next < _itemsSourceInternal.Count
                ? _itemsSourceInternal[next].StartTimeMs
                : syllable.OriginalElement.StartTimeMs;
        }

        private FormattedText GetOrCreateChordText(string chord)
        {
            if (_chordTextCache.TryGetValue(chord, out var text))
                return text;

            text = new FormattedText(
                chord,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                CurrentTypeface.GetValueOrDefault(),
                FontSize * ChordFontScale,
                ChordBrush ?? Brushes.Orange);

            _chordTextCache[chord] = text;
            return text;
        }

        #endregion
    }
}
