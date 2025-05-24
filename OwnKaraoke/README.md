# OwnKaraoke

A high-performance, cross-platform karaoke text display control for Avalonia UI applications with smooth scrolling animations and syllable-level highlighting.

## Features

- **Syllable-level highlighting** - Progressive text highlighting synchronized with timing data
- **Smooth scrolling animations** - Fluid line transitions with customizable animation speeds
- **Multi-line display** - Configurable number of visible lines with automatic line wrapping
- **Flexible styling** - Full control over fonts, colors, and text alignment
- **Performance optimized** - Advanced caching for FormattedText objects and typefaces
- **Cross-platform** - Built on Avalonia UI for Windows, macOS, and Linux support

## Installation

Add the `KaraokeDisplay` control to your Avalonia project by including the source files in your project.

## Quick Start

### Basic Usage

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:karaoke="clr-namespace:AvaloniaKaraoke">
    <karaoke:KaraokeDisplay x:Name="KaraokeControl"
                           VisibleLinesCount="3"
                           FontSize="24"
                           FontFamily="Arial"
                           TextAlignment="Center"
                           HighlightBrush="Yellow"
                           AlreadySungBrush="LightGoldenrodYellow"
                           Foreground="White" />
</Window>
```

### Code-behind Setup

```csharp
// Create timed text elements
var karaokeData = new List<TimedTextElement>
{
    new("Hello ", 0),
    new("world ", 1000),
    new("this ", 2000),
    new("is ", 2500),
    new("karaoke!", 3000),
    new("._.", 4000), // Line break marker
    new("Second ", 5000),
    new("line ", 6000),
    new("here", 7000)
};

// Set the data source
KaraokeControl.ItemsSource = karaokeData;

// Start playback
KaraokeControl.Start();
```

## Data Format

The control uses `TimedTextElement` records to define karaoke content:

```csharp
public record TimedTextElement(string Text, double StartTimeMs);
```

- **Text**: The text content for the syllable/word
- **StartTimeMs**: Absolute time in milliseconds when highlighting should begin

### Special Markers

- `"._."` - Line break marker to force a new line

### Example Data Structure

```csharp
var lyrics = new List<TimedTextElement>
{
    new("Twinkle ", 0),
    new("twinkle ", 800),
    new("little ", 1600),
    new("star", 2400),
    new("._.", 3200),           // Force line break
    new("How ", 3200),
    new("I ", 4000),
    new("wonder ", 4800),
    new("what ", 5600),
    new("you ", 6400),
    new("are", 7200)
};
```

## Properties

### Content Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ItemsSource` | `IEnumerable<TimedTextElement>?` | `null` | Collection of timed text elements |
| `VisibleLinesCount` | `int` | `3` | Number of lines visible simultaneously |

### Appearance Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `FontSize` | `double` | Inherited | Size of the text font |
| `FontFamily` | `FontFamily` | Inherited | Font family for text rendering |
| `FontWeight` | `FontWeight` | Inherited | Weight of the font (Bold, Normal, etc.) |
| `FontStyle` | `FontStyle` | Inherited | Style of the font (Italic, Normal, etc.) |
| `TextAlignment` | `TextAlignment` | `Left` | Horizontal alignment of text lines |

### Color Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Foreground` | `IBrush?` | Inherited | Color for unsung text |
| `HighlightBrush` | `IBrush?` | `Yellow` | Color for currently highlighted text |
| `AlreadySungBrush` | `IBrush?` | `LightGoldenrodYellow` | Color for already sung text |

## Methods

### Playback Control

```csharp
// Start karaoke from the beginning
karaokeControl.Start();

// Pause at current position
karaokeControl.Pause();

// Resume from paused position
karaokeControl.Resume();

// Stop and reset to beginning
karaokeControl.Stop();
```

## Advanced Usage

### Custom Styling

```xml
<karaoke:KaraokeDisplay ItemsSource="{Binding KaraokeData}"
                       VisibleLinesCount="4"
                       FontSize="32"
                       FontFamily="Impact"
                       FontWeight="Bold"
                       TextAlignment="Center"
                       HighlightBrush="#FF00FF"
                       AlreadySungBrush="#FFD700"
                       Foreground="#FFFFFF"
                       Background="#000000" />
```

### Dynamic Content Updates

```csharp
// Update content dynamically
var newLyrics = await LoadLyricsFromFile("song.lrc");
karaokeControl.ItemsSource = newLyrics;
karaokeControl.Start();
```

### Event Handling

The control automatically handles timing and animations. For custom timing control, manage the data source and call playback methods as needed.

## Performance Considerations

### Caching
- **FormattedText Caching**: Automatically caches formatted text objects for better performance
- **Typeface Caching**: Caches typeface objects to avoid repeated creation
- **Automatic Cache Management**: Implements intelligent cache size limits

### Optimization Tips
- Use consistent font properties when possible to maximize cache hits
- Avoid frequent changes to visual properties during playback
- Consider memory usage when working with very large lyric sets

## Animation Behavior

### Highlighting Animation
- Syllables highlight progressively from left to right
- Highlight timing is calculated based on the time difference between consecutive elements
- Default highlighting duration is 75% of the time to the next syllable (max 1000ms)

### Scrolling Animation
- Lines scroll smoothly when the highlight reaches approximately 1/3 through the second visible line
- Smooth easing animations for both position and opacity changes
- Configurable animation speeds via constants in the source code

### Animation Constants
```csharp
// Modifiable in source code
private const double LINE_ANIMATION_SPEED = 0.15;      // pixels per millisecond
private const double OPACITY_ANIMATION_SPEED = 0.00085; // opacity per millisecond
```

## Technical Details

### Architecture
- Built as a custom Avalonia `Control` with hardware-accelerated rendering
- Uses `DispatcherTimer` for smooth 60fps animations
- Implements efficient text measurement and layout algorithms

### Memory Management
- Automatic disposal of animation timers
- Intelligent cache cleanup to prevent memory leaks
- Optimized string building for line construction

### Thread Safety
- All operations are executed on the UI thread
- Safe to call from background threads (operations will be marshaled)

## Troubleshooting

### Common Issues

**Text not displaying**
- Ensure `ItemsSource` is set and contains valid data
- Check that `FontSize` is greater than 0
- Verify the control has non-zero dimensions

**Animation not working**
- Call `Start()` method after setting `ItemsSource`
- Ensure timing values in `TimedTextElement` are valid and increasing
- Check that the control is visible in the visual tree

**Performance issues**
- Monitor FormattedText cache size for very large datasets
- Consider reducing `VisibleLinesCount` for better performance
- Avoid frequent property changes during animation

### Debug Tips
- Check `_itemsSourceInternal.Count` to verify data loading
- Monitor `_currentGlobalSyllableIndex` for timing issues
- Use breakpoints in `OnFrame()` method to debug animation logic

## License

Include your license information here.

## Contributing

Guidelines for contributing to the OwnKaraoke control project.

## Version History

- **v1.0.0** - Initial release with core karaoke functionality
  - Syllable-level highlighting
  - Smooth scrolling animations
  - Multi-line display support
  - Comprehensive styling options