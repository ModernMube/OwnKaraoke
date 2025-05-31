# OwnKaraoke 
<a href="https://www.buymeacoffee.com/ModernMube">
  <img src="https://img.shields.io/badge/Support-Buy%20Me%20A%20Coffee-orange" alt="Buy Me a Coffee">
</a>

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

Add the OwnKaraoke project to your Avalonia solution and reference it in your main project:

```xml
<ProjectReference Include="..\OwnKaraoke\OwnKaraoke.csproj" />
```

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
[00:12.00]<00:12.17>Word1<00:12.33>Word2<00:12.99>Word3
[00:15.50]<00:16.43>Next<00:16.67>line<00:17.10>here
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
| `Status` | `KaraokeStatus` | `Idle` | Current playback status (Idle, Playing, Paused, Finished) |
| `Position` | `double` | `0.0` | Current position in milliseconds (tempo-adjusted) |
| `Duration` | `double` | `0.0` | Total duration in milliseconds (tempo-adjusted) |
| `OriginalPosition` | `double` | `0.0` | Current position in original time (without tempo) |
| `OriginalDuration` | `double` | `0.0` | Total duration in original time (without tempo) |

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

// Seek to specific position (milliseconds)
karaokeControl.Seek(30000); // Seek to 30 seconds
```

## Architecture

### Core Classes

- **`OwnKaraokeDisplay`** - Main control class with rendering and animation logic
- **`TimedTextElement`** - Record representing a single timed text element
- **`KaraokeLine`** - Internal class representing a display line with syllables
- **`SyllableMetrics`** - Internal class containing syllable positioning and formatting
- **`KaraokeStatus`** - Enum for playback status (Idle, Playing, Paused, Finished)
- **`OwnKaraokeLyric`** - Static parser class for LRC files
- **`TimerDisposable`** - Helper class for timer disposal

### Key Features

#### Advanced Scrolling Logic
The control implements intelligent scrolling with multiple conditions:
- Automatic scrolling when the first line is fully processed
- Smart handling of short lines (< 4 syllables)
- 1/3 rule for normal lines - scrolls when highlight reaches 1/3 into the second line
- Optimized seek positioning to ensure target syllables are visible

#### Performance Optimizations
- **FormattedText caching** - Reduces text rendering overhead
- **Typeface caching** - Avoids repeated typeface creation
- **Intelligent cache management** - Automatic cleanup when cache grows too large
- **Efficient string building** - Reusable StringBuilder for line construction

#### Real-time Tempo Processing
- **Thread-safe tempo changes** - Can be modified during playback
- **Dual timing system** - Tracks both original and tempo-adjusted time
- **Smooth transitions** - No interruption when tempo changes
- **Performance optimized** - Minimal CPU impact for tempo calculations

## Advanced Usage

### Custom Animation Constants

You can modify animation speeds by changing these constants in the source code:

```csharp
// In OwnKaraoke.cs
private const double LINE_ANIMATION_SPEED = 0.15;      // pixels per millisecond
private const double OPACITY_ANIMATION_SPEED = 0.00076; // opacity per millisecond
private const double FIXED_SPACE_WIDTH_FACTOR = 0.3;   // space width relative to font size
```

### Event Handling Example

```csharp
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Monitor status changes
        KaraokeControl.GetObservable(OwnKaraokeDisplay.StatusProperty)
            .Subscribe(status => 
            {
                switch (status)
                {
                    case KaraokeStatus.Playing:
                        Console.WriteLine("Karaoke started playing");
                        break;
                    case KaraokeStatus.Finished:
                        Console.WriteLine("Karaoke finished");
                        break;
                }
            });
    }
    
    private void SlowerButton_Click(object sender, RoutedEventArgs e)
    {
        KaraokeControl.Tempo = Math.Max(-2.0, KaraokeControl.Tempo - 0.1);
    }
    
    private void FasterButton_Click(object sender, RoutedEventArgs e)
    {
        KaraokeControl.Tempo = Math.Min(2.0, KaraokeControl.Tempo + 0.1);
    }
}
```

### Custom Styling

```xml
<karaoke:OwnKaraokeDisplay x:Name="KaraokeControl"
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

## Animation Behavior

### Highlighting Animation
- Syllables highlight progressively from left to right
- Highlight timing is calculated as 75% of the time to the next syllable (max 1000ms)
- Tempo adjustments apply in real-time to all timing calculations

### Scrolling Animation
- Lines scroll smoothly when highlight reaches approximately 1/3 through the second visible line
- Smooth easing animations for both position and opacity changes
- Tempo does not affect scrolling animations, only text highlighting timing

### Line Building
- Dynamic line wrapping based on available width
- Intelligent syllable fitting with overflow handling
- Support for line break markers (`"._."`)
- Automatic height and width calculation

## Technical Details

### Tempo Calculation
```csharp
// Tempo multiplier: 1.0 + Tempo value
double multiplier = 1.0 + Tempo;  // 0.0 = 1.0x, 0.5 = 1.5x, -0.3 = 0.7x

// Applied to timing calculations
double adjustedTime = originalTime / multiplier;     // Faster tempo = shorter intervals
double adjustedElapsed = elapsedTime * multiplier;   // Faster tempo = more elapsed time per frame
```

### Memory Management
- Automatic disposal of animation timers
- Intelligent cache cleanup to prevent memory leaks
- Optimized string building for line construction
- Thread-safe operations

### Performance Characteristics
- 60fps smooth animation using DispatcherTimer
- Hardware-accelerated rendering through Avalonia
- Minimal CPU usage for tempo calculations
- Efficient text measurement and layout algorithms

## Requirements

- **.NET 8.0** or later
- **Avalonia 11.3.0** or later
- **System.Reactive 6.0.1** or later

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

### Debug Tips
- Use `OwnKaraokeLyric.ExtractMetadata()` to verify LRC file parsing
- Monitor syllable index progression during playback
- Check tempo multiplier calculations with extreme values (±2.0)
- Use breakpoints in animation logic to debug timing issues

## Support

If you find this project helpful, consider supporting the development:

<a href="https://www.buymeacoffee.com/ModernMube" target="_blank">
    <img src="https://cdn.buymeacoffee.com/buttons/v2/arial-yellow.png" alt="Buy Me A Coffee" style="height: 60px !important;width: 217px !important;" >
</a>

## License

This project is open source. Please check the license file for more details.

## API Reference

### OwnKaraokeDisplay Class

Main karaoke display control with all styling, animation, and tempo control capabilities.

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