using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OwnKaraoke
{
    public partial class OwnKaraokeDisplay : Control
    {
        #region Typeface and Text Caching

        /// <summary>
        /// Gets the current typeface, using caching to improve performance.
        /// </summary>
        private Typeface? CurrentTypeface
        {
            get
            {
                if (_cachedTypeface == null ||
                    !ReferenceEquals(_lastFontFamily, FontFamily) ||
                    _lastFontStyle != FontStyle ||
                    _lastFontWeight != FontWeight)
                {
                    _cachedTypeface = new Typeface(FontFamily, FontStyle, FontWeight);
                    _lastFontFamily = FontFamily;
                    _lastFontStyle = FontStyle;
                    _lastFontWeight = FontWeight;
                }
                return _cachedTypeface;
            }
        }

        /// <summary>
        /// Gets or creates a FormattedText object from cache, creating new ones as needed.
        /// </summary>
        /// <param name="text">The text content.</param>
        /// <param name="brush">The brush to use for rendering.</param>
        /// <returns>A FormattedText object for the specified parameters.</returns>
        private FormattedText GetOrCreateFormattedText(string text, IBrush? brush)
        {
            var key = new FormattedTextKey(text, FontSize, brush, FontFamily, FontStyle, FontWeight);

            if (_formattedTextCache.TryGetValue(key, out var formattedText))
                return formattedText;

            if (CurrentTypeface is not null)
            {
                formattedText = new FormattedText(
                     text,
                     CultureInfo.CurrentUICulture,
                     FlowDirection.LeftToRight,
                     CurrentTypeface.Value,
                     FontSize,
                     brush ?? Brushes.White);
            }
            else
            {
                formattedText = new FormattedText(
                    text,
                    CultureInfo.CurrentUICulture,
                    FlowDirection.LeftToRight,
                    CurrentTypeface.GetValueOrDefault(),
                    FontSize,
                    brush ?? Brushes.White);
            }

            // Simple cache size management
            if (_formattedTextCache.Count > 1000)
            {
                var keysToRemove = _formattedTextCache.Keys.Take(200).ToArray();
                foreach (var keyToRemove in keysToRemove)
                    _formattedTextCache.Remove(keyToRemove);
            }

            _formattedTextCache[key] = formattedText;
            return formattedText;
        }

        /// <summary>
        /// Clears the FormattedText cache to free memory.
        /// </summary>
        private void ClearFormattedTextCache() => _formattedTextCache.Clear();

        #endregion

        #region Text Processing

        /// <summary>
        /// Calculates the metrics for a single syllable including width, height, and formatted text.
        /// </summary>
        /// <param name="originalText">The original text of the syllable.</param>
        /// <param name="actualFixedSpaceWidth">The width of space characters.</param>
        /// <returns>A tuple containing width, height, and FormattedText.</returns>
        private (double width, double height, FormattedText formattedText) CalculateSyllableMetrics(
            string originalText, double actualFixedSpaceWidth)
        {
            if (string.IsNullOrWhiteSpace(originalText))
            {
                var width = !string.IsNullOrEmpty(originalText) ? originalText.Length * actualFixedSpaceWidth : 0;
                var ft = GetOrCreateFormattedText(" ", Brushes.Transparent);
                var height = ft.Height > 0 ? ft.Height : FontSize * 1.2;
                return (width, height, ft);
            }

            var trimmedText = originalText.Trim();
            var formattedText = GetOrCreateFormattedText(trimmedText, Foreground ?? Brushes.White);
            var coreWidth = formattedText.Width;

            var leadingSpaces = CountLeadingSpaces(originalText);
            var trailingSpaces = CountTrailingSpaces(originalText);

            var totalWidth = (leadingSpaces + trailingSpaces) * actualFixedSpaceWidth + coreWidth;
            return (totalWidth, formattedText.Height, formattedText);
        }

        /// <summary>
        /// Counts the number of leading spaces in a text string.
        /// </summary>
        /// <param name="text">The text to analyze.</param>
        /// <returns>The number of leading spaces.</returns>
        private static int CountLeadingSpaces(string text)
        {
            var count = 0;
            for (var i = 0; i < text.Length && text[i] == ' '; i++)
                count++;
            return count;
        }

        /// <summary>
        /// Counts the number of trailing spaces in a text string.
        /// </summary>
        /// <param name="text">The text to analyze.</param>
        /// <returns>The number of trailing spaces.</returns>
        private static int CountTrailingSpaces(string text)
        {
            var count = 0;
            for (var i = text.Length - 1; i >= 0 && text[i] == ' '; i--)
                count++;
            return count;
        }

        /// <summary>
        /// Determines if a syllable can fit on the current line.
        /// </summary>
        /// <param name="line">The line being built.</param>
        /// <param name="syllableWidth">The width of the syllable to add.</param>
        /// <param name="currentLineWidth">The current width of the line.</param>
        /// <param name="availableWidth">The available width for the line.</param>
        /// <returns>True if the syllable can fit; otherwise, false.</returns>
        private static bool CanFitSyllable(KaraokeLine line, double syllableWidth,
            double currentLineWidth, double availableWidth)
        {
            if (line.Syllables.Count == 0)
                return true;

            return currentLineWidth + syllableWidth <= availableWidth;
        }

        /// <summary>
        /// Adds a syllable to the current line being built.
        /// </summary>
        /// <param name="line">The line to add the syllable to.</param>
        /// <param name="element">The timed text element.</param>
        /// <param name="syllableIdx">The global syllable index.</param>
        /// <param name="formattedText">The formatted text representation.</param>
        /// <param name="width">The width of the syllable.</param>
        /// <param name="height">The height of the syllable.</param>
        /// <param name="xOffset">The X offset within the line.</param>
        /// <param name="originalText">The original text content.</param>
        private void AddSyllableToLine(KaraokeLine line, TimedTextElement element, int syllableIdx,
            FormattedText formattedText, double width, double height, double xOffset, string originalText)
        {
            var metrics = new SyllableMetrics(element, syllableIdx, formattedText, width, height)
            {
                XOffsetInLine = xOffset
            };

            line.Syllables.Add(metrics);
            line.LastSyllableGlobalIndex = syllableIdx;

            if (!string.IsNullOrEmpty(originalText) && originalText != "._.")
                _stringBuilder.Append(originalText);
        }

        /// <summary>
        /// Finalizes a line after all syllables have been added.
        /// </summary>
        /// <param name="line">The line to finalize.</param>
        /// <param name="lineBreakEncountered">Whether a line break was encountered.</param>
        /// <param name="maxLineHeight">The maximum height of syllables in the line.</param>
        /// <returns>The finalized line or null if it should not be displayed.</returns>
        private KaraokeLine? FinalizeLine(KaraokeLine line, bool lineBreakEncountered, double maxLineHeight)
        {
            if (line.Syllables.Count > 0)
            {
                line.FormattedTextLine = GetOrCreateFormattedText(_stringBuilder.ToString(), Foreground ?? Brushes.White);
                line.LineHeight = maxLineHeight > 0 ? maxLineHeight : FontSize * 1.2;

                var lastSyllable = line.Syllables.LastOrDefault();
                line.LineWidth = lastSyllable != null ? lastSyllable.XOffsetInLine + lastSyllable.Width : 0;

                return line;
            }

            if (lineBreakEncountered)
            {
                line.FormattedTextLine = GetOrCreateFormattedText("", Brushes.Transparent);
                line.LineHeight = FontSize * 1.2;
                line.LineWidth = 0;
                line.LastSyllableGlobalIndex = line.FirstSyllableGlobalIndex;

                return line;
            }

            return null;
        }

        /// <summary>
        /// Sets the animation position for a line based on existing lines and animation state.
        /// </summary>
        /// <param name="line">The line to set position for.</param>
        /// <param name="lineNum">The line number in the display.</param>
        /// <param name="existingLines">The existing lines before rebuilding.</param>
        /// <param name="isScrollAnimation">Whether this is part of a scroll animation.</param>
        private void SetLineAnimationPosition(KaraokeLine line, int lineNum,
            List<KaraokeLine> existingLines, bool isScrollAnimation)
        {
            if (isScrollAnimation)
            {
                if (lineNum < existingLines.Count)
                {
                    line.CurrentY = existingLines[lineNum].CurrentY;
                    line.Opacity = existingLines[lineNum].Opacity;
                    line.TargetOpacity = existingLines[lineNum].TargetOpacity;
                }
                else
                {
                    line.CurrentY = line.TargetY;
                    line.Opacity = 0.0;
                    line.TargetOpacity = 1.0;
                    _isAnimatingLines = true;
                }
            }
            else
            {
                line.CurrentY = line.TargetY;
                line.Opacity = 1.0;
                line.TargetOpacity = 1.0;
            }
        }

        #endregion
    }
}
