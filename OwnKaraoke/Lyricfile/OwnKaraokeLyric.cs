using global::OwnKaraoke;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OwnKaraoke.Lyricfile
{
    /// <summary>
    /// Parser for LRC (Lyric) files with syllable-level timing support for OwnKaraoke control.
    /// Supports standard LRC format with enhanced word-level timing information.
    /// </summary>
    public static class OwnKaraokeLyric
    {
        /// <summary>
        /// Regular expression pattern for matching LRC metadata lines (e.g., [ar:Artist], [ti:Title]).
        /// </summary>
        private static readonly Regex MetadataPattern = new Regex(
            @"^\[(?:ar|ti|al|au|by|re|ve|length):",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Regular expression pattern for matching LRC timing lines with optional word-level timing.
        /// Format: [mm:ss.ff]text or [mm:ss.ff]&lt;mm:ss.ff&gt;word&lt;mm:ss.ff&gt;word...
        /// </summary>
        private static readonly Regex TimingLinePattern = new Regex(
            @"^\[(\d{1,2}:\d{2}\.\d{2})\]\s*(.*)$",
            RegexOptions.Compiled);

        /// <summary>
        /// Regular expression pattern for matching individual word timing within a line.
        /// Format: &lt;mm:ss.ff&gt;word
        /// </summary>
        private static readonly Regex WordTimingPattern = new Regex(
            @"<(\d{1,2}:\d{2}\.\d{2})>([^<]*)",
            RegexOptions.Compiled);

        /// <summary>
        /// Parses an LRC file from the specified file path and returns timed text elements for OwnKaraoke.
        /// </summary>
        /// <param name="filePath">The path to the LRC file to parse.</param>
        /// <returns>A collection of TimedTextElement objects suitable for OwnKaraoke display.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist.</exception>
        /// <exception cref="InvalidDataException">Thrown when the LRC file format is invalid.</exception>
        public static async Task<IEnumerable<TimedTextElement>> ParseFromFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"LRC file not found: {filePath}");

            try
            {
                var content = await File.ReadAllTextAsync(filePath);
                return ParseFromString(content);
            }
            catch (Exception ex) when (!(ex is FileNotFoundException))
            {
                throw new InvalidDataException($"Error reading LRC file: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Parses an LRC file from the specified file path synchronously and returns timed text elements for OwnKaraoke.
        /// </summary>
        /// <param name="filePath">The path to the LRC file to parse.</param>
        /// <returns>A collection of TimedTextElement objects suitable for OwnKaraoke display.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist.</exception>
        /// <exception cref="InvalidDataException">Thrown when the LRC file format is invalid.</exception>
        public static IEnumerable<TimedTextElement> ParseFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"LRC file not found: {filePath}");

            try
            {
                var content = File.ReadAllText(filePath);
                return ParseFromString(content);
            }
            catch (Exception ex) when (!(ex is FileNotFoundException))
            {
                throw new InvalidDataException($"Error reading LRC file: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Parses LRC content from a string and returns timed text elements for OwnKaraoke.
        /// </summary>
        /// <param name="lrcContent">The LRC file content as a string.</param>
        /// <returns>A collection of TimedTextElement objects suitable for OwnKaraoke display.</returns>
        /// <exception cref="InvalidDataException">Thrown when the LRC content format is invalid.</exception>
        public static IEnumerable<TimedTextElement> ParseFromString(string lrcContent)
        {
            if (string.IsNullOrWhiteSpace(lrcContent))
                return Enumerable.Empty<TimedTextElement>();

            try
            {
                var lines = lrcContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var timedElements = new List<TimedTextElement>();

                foreach (var line in lines)
                {
                    // Skip metadata lines
                    if (IsMetadataLine(line))
                        continue;

                    // Skip empty lines
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    // Parse timing lines
                    var lineElements = ParseTimingLine(line);
                    timedElements.AddRange(lineElements);
                }

                // Sort by start time to ensure proper ordering
                return timedElements.OrderBy(e => e.StartTimeMs).ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidDataException($"Error parsing LRC content: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Determines if a line is a metadata line that should be skipped during parsing.
        /// </summary>
        /// <param name="line">The line to check.</param>
        /// <returns>True if the line is metadata; otherwise, false.</returns>
        private static bool IsMetadataLine(string line)
        {
            return MetadataPattern.IsMatch(line.Trim());
        }

        /// <summary>
        /// Parses a single timing line and extracts all timed text elements from it.
        /// Supports both simple line timing [mm:ss.ff]text and word-level timing [mm:ss.ff]&lt;mm:ss.ff&gt;word.
        /// </summary>
        /// <param name="line">The timing line to parse.</param>
        /// <returns>A collection of TimedTextElement objects from the line.</returns>
        private static IEnumerable<TimedTextElement> ParseTimingLine(string line)
        {
            var match = TimingLinePattern.Match(line.Trim());
            if (!match.Success)
                return Enumerable.Empty<TimedTextElement>();

            var lineStartTime = ParseTimeToMilliseconds(match.Groups[1].Value);
            if (lineStartTime < 0)
                return Enumerable.Empty<TimedTextElement>();

            var textContent = match.Groups[2].Value.Trim();
            if (string.IsNullOrEmpty(textContent))
                return Enumerable.Empty<TimedTextElement>();

            var elements = new List<TimedTextElement>();

            // Check if this line has word-level timing (contains < and > characters)
            if (textContent.Contains('<') && textContent.Contains('>'))
            {
                // Parse word-level timing
                var wordMatches = WordTimingPattern.Matches(textContent);

                foreach (Match wordMatch in wordMatches)
                {
                    var wordTime = ParseTimeToMilliseconds(wordMatch.Groups[1].Value);
                    var wordText = wordMatch.Groups[2].Value;

                    // Use the word-specific timing if available, otherwise use line start time
                    var finalTime = wordTime >= 0 ? wordTime : lineStartTime;

                    // Add the word with proper spacing if it's not empty
                    if (!string.IsNullOrEmpty(wordText))
                    {
                        elements.Add(new TimedTextElement(wordText.Trim() + ' ', finalTime));
                    }
                }
            }
            else
            {
                // Simple line timing - split by spaces and create individual word elements
                var words = textContent.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (words.Length == 0)
                    return Enumerable.Empty<TimedTextElement>();

                // Calculate timing distribution across words
                var baseTimePerWord = 500; // 500ms per word as default

                for (int i = 0; i < words.Length; i++)
                {
                    var wordTime = lineStartTime + (i * baseTimePerWord);
                    var wordText = words[i];

                    // Add space after word (except for last word)
                    if (i < words.Length - 1)
                        wordText += " ";

                    elements.Add(new TimedTextElement(wordText.Trim() + ' ', wordTime));
                }
            }

            // Add line break marker after each line (except if line is empty)
            if (elements.Count > 0)
            {
                var lastElementTime = elements.LastOrDefault()?.StartTimeMs ?? lineStartTime;
                elements.Add(new TimedTextElement("._.", lastElementTime + 200)); // Add 200ms after last word
            }

            return elements;
        }

        /// <summary>
        /// Parses a time string in LRC format (mm:ss.ff or m:ss.ff) to milliseconds.
        /// </summary>
        /// <param name="timeString">The time string to parse (e.g., "01:23.45" or "1:23.45").</param>
        /// <returns>The time in milliseconds, or -1 if parsing fails.</returns>
        private static double ParseTimeToMilliseconds(string timeString)
        {
            try
            {
                // Expected format: mm:ss.ff or m:ss.ff (minutes:seconds.centiseconds)
                var parts = timeString.Split(':');
                if (parts.Length != 2)
                    return -1;

                if (!int.TryParse(parts[0], out var minutes))
                    return -1;

                var secondsParts = parts[1].Split('.');
                if (secondsParts.Length != 2)
                    return -1;

                if (!int.TryParse(secondsParts[0], out var seconds))
                    return -1;

                if (!int.TryParse(secondsParts[1], out var centiseconds))
                    return -1;

                // Convert to milliseconds
                var totalMilliseconds = (minutes * 60 * 1000) + (seconds * 1000) + (centiseconds * 10);
                return totalMilliseconds;
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// Extracts metadata information from an LRC file.
        /// </summary>
        /// <param name="lrcContent">The LRC file content.</param>
        /// <returns>A dictionary containing metadata key-value pairs.</returns>
        public static Dictionary<string, string> ExtractMetadata(string lrcContent)
        {
            var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(lrcContent))
                return metadata;

            var lines = lrcContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith("[") && trimmedLine.Contains(":") && trimmedLine.EndsWith("]"))
                {
                    // Extract metadata like [ar:Artist], [ti:Title], etc.
                    var colonIndex = trimmedLine.IndexOf(':');
                    if (colonIndex > 1 && colonIndex < trimmedLine.Length - 2)
                    {
                        var key = trimmedLine.Substring(1, colonIndex - 1);
                        var value = trimmedLine.Substring(colonIndex + 1, trimmedLine.Length - colonIndex - 2);

                        metadata[key] = value;
                    }
                }
            }

            return metadata;
        }

        /// <summary>
        /// Validates if the provided content is a valid LRC file format.
        /// </summary>
        /// <param name="content">The content to validate.</param>
        /// <returns>True if the content appears to be valid LRC format; otherwise, false.</returns>
        public static bool IsValidLrcFormat(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return false;

            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Check if at least one line contains timing information
            return lines.Any(line => TimingLinePattern.IsMatch(line.Trim()));
        }
    }

    /// <summary>
    /// Extension methods for easier LRC parsing integration.
    /// </summary>
    public static class LrcParserExtensions
    {
        /// <summary>
        /// Loads LRC content directly into a KaraokeDisplay control.
        /// </summary>
        /// <param name="karaokeDisplay">The KaraokeDisplay control to load content into.</param>
        /// <param name="lrcFilePath">The path to the LRC file.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task LoadFromLrcFileAsync(this OwnKaraokeDisplay karaokeDisplay, string? lrcFilePath)
        {
            var timedElements = await OwnKaraokeLyric.ParseFromFileAsync(lrcFilePath);
            karaokeDisplay.ItemsSource = timedElements;
        }

        /// <summary>
        /// Loads LRC content directly into a KaraokeDisplay control synchronously.
        /// </summary>
        /// <param name="karaokeDisplay">The KaraokeDisplay control to load content into.</param>
        /// <param name="lrcFilePath">The path to the LRC file.</param>
        public static void LoadFromLrcFile(this OwnKaraokeDisplay karaokeDisplay, string? lrcFilePath)
        {
            if(lrcFilePath is not null)
            {
                var timedElements = OwnKaraokeLyric.ParseFromFile(lrcFilePath);
                karaokeDisplay.ItemsSource = timedElements;
            }             
        }

        /// <summary>
        /// Loads LRC content from a string directly into a KaraokeDisplay control.
        /// </summary>
        /// <param name="karaokeDisplay">The KaraokeDisplay control to load content into.</param>
        /// <param name="lrcContent">The LRC content as a string.</param>
        public static void LoadFromLrcString(this OwnKaraokeDisplay karaokeDisplay, string lrcContent)
        {
            var timedElements = OwnKaraokeLyric.ParseFromString(lrcContent);
            karaokeDisplay.ItemsSource = timedElements;
        }
    }
}
