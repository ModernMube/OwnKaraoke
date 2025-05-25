# OwnKaraoke 
<a href="https://www.buymeacoffee.com/ModernMube">
  <img src="https://img.shields.io/badge/Support-Buy%20Me%20A%20Coffe-orange" alt="Buy Me a Coffe">
</a>

##

A high-performance, cross-platform karaoke text display control for Avalonia UI applications with smooth scrolling animations, syllable-level highlighting, and real-time tempo control.

## Features

- **Syllable-level highlighting** - Progressive text highlighting synchronized with timing data
- **Smooth scrolling animations** - Fluid line transitions with customizable animation speeds
- **Real-time tempo control** - Adjust playback speed from -200% to +200% during playback
- **Multi-line display** - Configurable number of visible lines with automatic line wrapping
- **Flexible styling** - Full control over fonts, colors, and text alignment
- **Performance optimized** - Advanced caching for FormattedText objects and typefaces
- **Cross-platform** - Built on Avalonia UI for Windows, macOS, and Linux support
- **LRC file support** - Built-in parser for standard LRC lyric files with word-level timing

## Installation

Add the control files to your Avalonia project:
- `AvaloniaKaraokeControl.cs` - Main karaoke display control
- `OwnKaraokeLyric.cs` - LRC file parser for lyric loading

## Quick Start

### Basic Usage

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:karaoke="clr-namespace:OwnKaraoke;assembly=OwnKaraoke">
    <karaoke:OwnKaraokeDisplay x:Name="KaraokeControl"
                              VisibleLinesCount="3"
                              FontSize="24"
                              FontFamily="Arial"
                              TextAlignment="Center"
                              Tempo="0.0"
                              HighlightBrush="Yellow"
                              AlreadySungBrush="LightGoldenrodYellow"
                              Foreground="White" />
</Window>
```

### Real-time Tempo Control

```xml
<Grid>
    <!-- Tempo control slider -->
    <StackPanel Orientation="Horizontal" Margin="10" VerticalAlignment="Top">
        <TextBlock Text="Tempo:" VerticalAlignment="Center" Margin="0,0,10,0"/>
        <Slider Name="TempoSlider" 
                Minimum="-2.0" 
                Maximum="2.0" 
                Value="0.0"
                Width="200"
                TickFrequency="0.1"
                IsSnapToTickEnabled="True"
                VerticalAlignment="Center"/>
        <TextBlock Text="{Binding #TempoSlider.Value, StringFormat={}{0:F1}}" 
                   VerticalAlignment="Center" 
                   Margin="10,0,0,0"/>
        <TextBlock Name="TempoPercentage" 
                   VerticalAlignment="Center" 
                   Margin="10,0,0,0"/>
    </StackPanel>

    <!-- Karaoke display with tempo binding -->
    <karaoke:OwnKaraokeDisplay x:Name="KaraokeControl"
                              Tempo="{Binding #TempoSlider.Value}"
                              VisibleLinesCount="3"
                              FontSize="24"
                              TextAlignment="Center" />
</Grid>
```

### Loading from LRC File

```csharp
// Load directly from LRC file (async)
await KaraokeControl.LoadFromLrcFileAsync("song.lrc");
KaraokeControl.Start();

// Or synchronously
KaraokeControl.LoadFromLrcFile("song.lrc");
KaraokeControl.Start();
```

### Manual Setup with TimedTextElement

```csharp
// Create timed text elements manually
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

// Set initial tempo (optional)
KaraokeControl.Tempo = 0.0; // Normal speed

// Start playback
KaraokeControl.Start();
```

## Tempo Control

The control supports real-time tempo adjustment during playback without stopping or restarting the animation.

### Tempo Range and Values

- **Range**: -2.0 to +2.0
- **Scale**: Each 0.1 increment = 10% speed change
- **Examples**:
  - `0.0` = Normal speed (100%)
  - `0.5` = 50% faster (150% speed)
  - `-0.3` = 30% slower (70% speed)
  - `1.0` = 100% faster (200% speed)
  - `-1.0` = 100% slower (50% speed)

### Programmatic Tempo Control

```csharp
// Set tempo programmatically
KaraokeControl.Tempo = 0.2;  // 20% faster
KaraokeControl.Tempo = -0.1; // 10% slower
KaraokeControl.Tempo = 0.0;  // Normal speed
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

## LRC File Support

OwnKaraoke includes built-in support for LRC (Lyric) files with word-level timing. The `OwnKaraokeLyric` class provides comprehensive parsing capabilities.

### LRC File Format

The parser supports standard LRC format with enhanced word-level timing:

```
[ar:Artist Name]
[ti:Song Title]
[00:12.00]<00:12.17>Egy<00:12.33>hálás<00:12.99>szív
[00:15.50]<00:16.43>Egy<00:16.67>hálás<00:17.10>szív
```

### Loading LRC Files

```csharp
// Using extension methods (recommended)
await KaraokeControl.LoadFromLrcFileAsync("path/to/song.lrc");

// Or using the parser directly
var lyrics = await OwnKaraokeLyric.ParseFromFileAsync("path/to/song.lrc");
KaraokeControl.ItemsSource = lyrics;

// Synchronous loading
KaraokeControl.LoadFromLrcFile("path/to/song.lrc");

// From string content
string lrcContent = File.ReadAllText("song.lrc");
KaraokeControl.LoadFromLrcString(lrcContent);
```

### LRC Metadata Extraction

```csharp
string lrcContent = File.ReadAllText("song.lrc");
var metadata = OwnKaraokeLyric.ExtractMetadata(lrcContent);

// Access metadata
string artist = metadata.GetValueOrDefault("ar", "Unknown Artist");
string title = metadata.GetValueOrDefault("ti", "Unknown Title");
```

### LRC Validation

```csharp
string content = File.ReadAllText("file.lrc");
if (OwnKaraokeLyric.IsValidLrcFormat(content))
{
    // Valid LRC file, proceed with parsing
    var lyrics = OwnKaraokeLyric.ParseFromString(content);
}
```

## Properties

### Content Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ItemsSource` | `IEnumerable<TimedTextElement>?` | `null` | Collection of timed text elements |
| `VisibleLinesCount` | `int` | `3` | Number of lines visible simultaneously |

### Playback Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Tempo` | `double` | `0.0` | Playback speed adjustment (-2.0 to +2.0, where 0.1 = 10% change) |

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

### Custom Styling with Tempo Control

```xml
<Grid>
    <!-- Tempo controls -->
    <StackPanel Orientation="Horizontal" VerticalAlignment="Top" HorizontalAlignment="Center">
        <Button Content="Slower" Click="SlowerButton_Click" Margin="5"/>
        <TextBlock Text="{Binding #KaraokeControl.Tempo, StringFormat=Tempo: {0:F1}}" 
                   VerticalAlignment="Center" Margin="10"/>
        <Button Content="Faster" Click="FasterButton_Click" Margin="5"/>
        <Button Content="Normal" Click="NormalButton_Click" Margin="5"/>
    </StackPanel>

    <!-- Karaoke display -->
    <karaoke:OwnKaraokeDisplay x:Name="KaraokeControl"
                              ItemsSource="{Binding KaraokeData}"
                              VisibleLinesCount="4"
                              FontSize="32"
                              FontFamily="Impact"
                              FontWeight="Bold"
                              TextAlignment="Center"
                              Tempo="{Binding SelectedTempo}"
                              HighlightBrush="#FF00FF"
                              AlreadySungBrush="#FFD700"
                              Foreground="#FFFFFF"
                              Background="#000000" />
</Grid>
```

### Dynamic Content Updates with Tempo

```csharp
// Update content dynamically from LRC file
await karaokeControl.LoadFromLrcFileAsync("newsong.lrc");
karaokeControl.Tempo = 0.1; // Start 10% faster
karaokeControl.Start();

// Or manually
var newLyrics = await OwnKaraokeLyric.ParseFromFileAsync("song.lrc");
karaokeControl.ItemsSource = newLyrics;
karaokeControl.Tempo = -0.2; // Start 20% slower
karaokeControl.Start();
```

### Event Handling

The control automatically handles timing and animations. The tempo can be adjusted in real-time without affecting the animation state.

## Performance Considerations

### Caching
- **FormattedText Caching**: Automatically caches formatted text objects for better performance
- **Typeface Caching**: Caches typeface objects to avoid repeated creation
- **Automatic Cache Management**: Implements intelligent cache size limits

### Optimization Tips
- Use consistent font properties when possible to maximize cache hits
- Avoid frequent changes to visual properties during playback
- Consider memory usage when working with very large lyric sets
- Tempo changes are lightweight and don't affect performance

## Animation Behavior

### Highlighting Animation
- Syllables highlight progressively from left to right
- Highlight timing is calculated based on the time difference between consecutive elements
- Default highlighting duration is 75% of the time to the next syllable (max 1000ms)
- **Tempo adjustments apply in real-time** to all timing calculations

### Scrolling Animation
- Lines scroll smoothly when the highlight reaches approximately 1/3 through the second visible line
- Smooth easing animations for both position and opacity changes
- Configurable animation speeds via constants in the source code
- **Tempo does not affect scrolling animations**, only text highlighting timing

### Animation Constants
```csharp
// Modifiable in source code
private const double LINE_ANIMATION_SPEED = 0.15;      // pixels per millisecond
private const double OPACITY_ANIMATION_SPEED = 0.00076; // opacity per millisecond
```

### Tempo Calculation Details
```csharp
// Tempo multiplier calculation
double multiplier = 1.0 + Tempo;  // 0.0 = 1.0x, 0.5 = 1.5x, -0.3 = 0.7x

// Applied to timing calculations
double adjustedTime = originalTime / multiplier;  // Faster tempo = shorter time intervals
double adjustedElapsed = elapsedTime * multiplier; // Faster tempo = more elapsed time per frame
```

## Technical Details

### Architecture
- Built as a custom Avalonia `Control` with hardware-accelerated rendering
- Uses `DispatcherTimer` for smooth 60fps animations
- Implements efficient text measurement and layout algorithms
- **Real-time tempo processing** with minimal performance impact

### Memory Management
- Automatic disposal of animation timers
- Intelligent cache cleanup to prevent memory leaks
- Optimized string building for line construction

### Thread Safety
- All operations are executed on the UI thread
- Safe to call from background threads (operations will be marshaled)
- **Tempo changes are thread-safe** and applied immediately

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

**Tempo not working**
- Verify tempo value is within valid range (-2.0 to +2.0)
- Check that animation is running (tempo only affects active playback)
- Ensure timing data in `TimedTextElement` is properly formatted

**LRC file not loading**
- Verify the LRC file exists and is accessible
- Check file format with `OwnKaraokeLyric.IsValidLrcFormat()`
- Ensure the file contains valid timing information
- Check for proper word-level timing format: `<mm:ss.ff>word`

**Performance issues**
- Monitor FormattedText cache size for very large datasets
- Consider reducing `VisibleLinesCount` for better performance
- Avoid frequent property changes during animation
- Tempo changes are lightweight and should not cause performance issues

### Debug Tips
- Use `OwnKaraokeLyric.ExtractMetadata()` to verify LRC file parsing
- Check `_itemsSourceInternal.Count` to verify data loading
- Monitor `_currentGlobalSyllableIndex` for timing issues
- Use breakpoints in `OnFrame()` method to debug animation logic
- **Test tempo values**: Use extreme values (±2.0) to verify tempo is working
- **Monitor tempo multiplier**: Check `GetTempoMultiplier()` return value

### LRC File Format Requirements
- Lines must follow the pattern: `[mm:ss.ff]<mm:ss.ff>word<mm:ss.ff>word...`
- Time format: minutes:seconds.centiseconds (e.g., `01:23.45`)
- Metadata lines start with `[ar:`, `[ti:`, `[al:`, etc.
- Empty lines and metadata lines are automatically skipped

### Tempo Feature Notes
- **Real-time adjustment**: Tempo can be changed during playback without interruption
- **Range validation**: Values outside -2.0 to +2.0 are automatically clamped
- **Precision**: Use increments of 0.1 for 10% speed changes
- **Performance**: Tempo calculations are optimized for minimal CPU impact

## Support My Work

If you find this project helpful, consider buying me a coffee!

<a href="https://www.buymeacoffee.com/ModernMube" 
    target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/v2/arial-yellow.png" 
    alt="Buy Me A Coffee" 
    style="height: 60px !important;width: 217px !important;" >
 </a>

## Version History

- **v1.1.0** - Added real-time tempo control
  - Real-time tempo adjustment from -200% to +200% speed
  - Tempo property with data binding support
  - Thread-safe tempo changes during playback
  - Optimized tempo calculation algorithms
- **v1.0.0** - Initial release with core karaoke functionality
  - Syllable-level highlighting
  - Smooth scrolling animations
  - Multi-line display support
  - Comprehensive styling options
  - Built-in LRC file parser (`OwnKaraokeLyric`)
  - Word-level timing support
  - Metadata extraction from LRC files
  - Extension methods for easy integration

## API Reference

### OwnKaraokeDisplay Class

Main karaoke display control with all styling, animation, and tempo control capabilities.

#### New Properties (v1.1.0)
- `Tempo` (double): Real-time playback speed control (-2.0 to +2.0)

#### New Methods (v1.1.0)
- Tempo calculations are handled internally, no new public methods

### OwnKaraokeLyric Class

Static parser class for LRC files:

```csharp
// Main parsing methods
Task<IEnumerable<TimedTextElement>> ParseFromFileAsync(string filePath)
IEnumerable<TimedTextElement> ParseFromFile(string filePath)
IEnumerable<TimedTextElement> ParseFromString(string lrcContent)

// Utility methods
Dictionary<string, string> ExtractMetadata(string lrcContent)
bool IsValidLrcFormat(string content)
```

### Extension Methods

```csharp
// For OwnKaraokeDisplay
Task LoadFromLrcFileAsync(string lrcFilePath)
void LoadFromLrcFile(string lrcFilePath)
void LoadFromLrcString(string lrcContent)
```
