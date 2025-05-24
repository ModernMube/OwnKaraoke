using Avalonia.Media;
using Avalonia.Threading;
using System;

namespace OwnKaraoke
{
    /// <summary>
    /// Represents a timed text element with text content and absolute timing position for karaoke display.
    /// </summary>
    /// <param name="Text">The text content of the element</param>
    /// <param name="StartTimeMs">The absolute time in milliseconds when this element should start highlighting</param>
    public record TimedTextElement(string Text, double StartTimeMs);

    /// <summary>
    /// Key for FormattedText caching to optimize text rendering performance.
    /// </summary>
    internal readonly struct FormattedTextKey : IEquatable<FormattedTextKey>
    {
        /// <summary>
        /// The text content.
        /// </summary>
        public readonly string Text;

        /// <summary>
        /// The font size.
        /// </summary>
        public readonly double FontSize;

        /// <summary>
        /// The brush used for rendering.
        /// </summary>
        public readonly IBrush? Brush;

        /// <summary>
        /// The font family.
        /// </summary>
        public readonly FontFamily FontFamily;

        /// <summary>
        /// The font style.
        /// </summary>
        public readonly FontStyle FontStyle;

        /// <summary>
        /// The font weight.
        /// </summary>
        public readonly FontWeight FontWeight;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormattedTextKey"/> struct.
        /// </summary>
        /// <param name="text">The text content.</param>
        /// <param name="fontSize">The font size.</param>
        /// <param name="brush">The brush for rendering.</param>
        /// <param name="fontFamily">The font family.</param>
        /// <param name="fontStyle">The font style.</param>
        /// <param name="fontWeight">The font weight.</param>
        public FormattedTextKey(string text, double fontSize, IBrush? brush,
            FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight)
        {
            Text = text;
            FontSize = fontSize;
            Brush = brush;
            FontFamily = fontFamily;
            FontStyle = fontStyle;
            FontWeight = fontWeight;
        }

        /// <summary>
        /// Determines whether the specified <see cref="FormattedTextKey"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The other key to compare.</param>
        /// <returns>True if the keys are equal; otherwise, false.</returns>
        public bool Equals(FormattedTextKey other) =>
            Text == other.Text &&
            FontSize.Equals(other.FontSize) &&
            ReferenceEquals(Brush, other.Brush) &&
            ReferenceEquals(FontFamily, other.FontFamily) &&
            FontStyle.Equals(other.FontStyle) &&
            FontWeight.Equals(other.FontWeight);

        /// <summary>
        /// Determines whether the specified object is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>True if the objects are equal; otherwise, false.</returns>
        public override bool Equals(object? obj) => obj is FormattedTextKey other && Equals(other);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode() => HashCode.Combine(Text, FontSize, Brush, FontFamily, FontStyle, FontWeight);
    }
}
