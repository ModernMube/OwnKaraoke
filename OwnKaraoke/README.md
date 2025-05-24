# OwnKaraoke 

## Features

- **Syllable-level highlighting** - Progressive text highlighting synchronized with timing data
- **Smooth scrolling animations** - Fluid line transitions with customizable animation speeds
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
                              HighlightBrush="Yellow"
                              AlreadySungBrush="LightGoldenrodYellow"
                              Foreground="White" />
</Window>
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
<karaoke:OwnKaraokeDisplay ItemsSource="{Binding KaraokeData}"
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
// Update content dynamically from LRC file
await karaokeControl.LoadFromLrcFileAsync("newsong.lrc");
karaokeControl.Start();

// Or manually
var newLyrics = await OwnKaraokeLyric.ParseFromFileAsync("song.lrc");
karaokeControl.ItemsSource = newLyrics;
karaokeControl.Start();
```

### Complete Example with LRC Loading

```csharp
public partial class MainWindow : Window
{
    private OwnKaraokeDisplay _karaokeControl;
    
    public MainWindow()
    {
        InitializeComponent();
        _karaokeControl = this.FindControl<OwnKaraokeDisplay>("KaraokeControl");
    }
    
    private async void LoadSong_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Load LRC file
            await _karaokeControl.LoadFromLrcFileAsync("Assets/song.lrc");
            
            // Start karaoke
            _karaokeControl.Start();
        }
        catch (FileNotFoundException)
        {
            // Handle file not found
            Console.WriteLine("LRC file not found!");
        }
        catch (InvalidDataException ex)
        {
            // Handle invalid LRC format
            Console.WriteLine($"Invalid LRC format: {ex.Message}");
        }
    }
    
    private void Pause_Click(object sender, RoutedEventArgs e)
    {
        _karaokeControl.Pause();
    }
    
    private void Resume_Click(object sender, RoutedEventArgs e)
    {
        _karaokeControl.Resume();
    }
    
    private void Stop_Click(object sender, RoutedEventArgs e)
    {
        _karaokeControl.Stop();
    }
}
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

**LRC file not loading**
- Verify the LRC file exists and is accessible
- Check file format with `OwnKaraokeLyric.IsValidLrcFormat()`
- Ensure the file contains valid timing information
- Check for proper word-level timing format: `<mm:ss.ff>word`

**Performance issues**
- Monitor FormattedText cache size for very large datasets
- Consider reducing `VisibleLinesCount` for better performance
- Avoid frequent property changes during animation

### Debug Tips
- Use `OwnKaraokeLyric.ExtractMetadata()` to verify LRC file parsing
- Check `_itemsSourceInternal.Count` to verify data loading
- Monitor `_currentGlobalSyllableIndex` for timing issues
- Use breakpoints in `OnFrame()` method to debug animation logic

### LRC File Format Requirements
- Lines must follow the pattern: `[mm:ss.ff]<mm:ss.ff>word<mm:ss.ff>word...`
- Time format: minutes:seconds.centiseconds (e.g., `01:23.45`)
- Metadata lines start with `[ar:`, `[ti:`, `[al:`, etc.
- Empty lines and metadata lines are automatically skipped

## License

Include your license information here.

## Support My Work

If you find this project helpful, consider buying me a coffee!

<a href="https://www.buymeacoffee.com/ModernMube" 
    target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/v2/arial-yellow.png" 
    alt="Buy Me A Coffee" 
    style="height: 60px !important;width: 217px !important;" >
 </a>

## Version History

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

Main karaoke display control with all styling and animation capabilities.

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
