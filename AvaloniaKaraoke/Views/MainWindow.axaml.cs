using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using OwnKaraoke;
using OwnKaraoke.Lyricfile;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AvaloniaKaraoke.Views
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<TimedTextElement> _karaokeLyrics = new();
        private DispatcherTimer? _statusTimer;
        private DateTime _startTime;
        private bool _isPlaying = false;

        public ObservableCollection<TimedTextElement> KaraokeLyrics
        {
            get => _karaokeLyrics;
            set
            {
                _karaokeLyrics = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            
            InitializeComponent();            

            DataContext = this;  

            InitializeSongs();
            SetupStatusTimer();

            TempoSlider.PropertyChanged += OnTempoSliderChanged;

            UpdateTempoPercentage(0.0);
        }

        private void InitializeSongs()
        {
            var songs = new List<string>
            {
                "Hallelujah - Leonard Cohen",
                "Bohemian Rhapsody - Queen", 
                "Hotel California - Eagles",
                "Load lyric file"
            };

            SongSelector.ItemsSource = songs;
        }

        /// <summary>
        /// Sets the type of audio files that can be opened.
        /// </summary>
        private FilePickerFileType _lyricFiles { get; } = new("Lyric File")
        {
            Patterns = new[] { "*.lrc" },
            AppleUniformTypeIdentifiers = new[] { "public.text" },
            MimeTypes = new[] { "text/plain" }
        };

        private void SetupStatusTimer()
        {
            _statusTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _statusTimer.Tick += UpdateStatus;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (KaraokeLyrics.Any())
            { 
                KaraokeControl.Start();
                _isPlaying = true;
                _startTime = DateTime.Now;
                _statusTimer?.Start();
                
                StatusText.Text = "Playing...";
                StartButton.IsEnabled = false;
                StopButton.IsEnabled = true;
            }
            else
            {
                StatusText.Text = "Please choose a song!";
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _isPlaying = false;
            _statusTimer?.Stop();
            
            StatusText.Text = "Stopped";
            ProgressText.Text = "";
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            KaraokeControl?.Stop();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            StopButton_Click(sender, e);
            
            if (KaraokeLyrics.Any())
            {
                var currentSong = SongSelector.SelectedItem?.ToString();
                if (currentSong is not null)
                    LoadSongLyrics(currentSong);
            }
            
            StatusText.Text = "Restarted - Ready to start";
        }

        private void SongSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SongSelector.SelectedItem is string selectedSong)
            {
                LoadSongLyrics(selectedSong);
                StatusText.Text = $"Song loaded: {selectedSong}";
                StartButton.IsEnabled = true;
                
                if (_isPlaying)
                {
                    StopButton_Click(sender, new RoutedEventArgs());
                }
            }
        }

        private async Task LoadSongLyrics(string songName)
        {
            KaraokeLyrics.Clear();

            List<TimedTextElement> lyrics;

            if (songName == "Hallelujah - Leonard Cohen")
            {
                lyrics = GetHallelujahLyrics();
            }
            else if (songName == "Bohemian Rhapsody - Queen")
            {
                lyrics = GetBohemianRhapsodyLyrics();
            }
            else if (songName == "Hotel California - Eagles")
            {
                lyrics = GetHotelCaliforniaLyrics();
            }
            else
            {
                lyrics = await GetDefaultLyrics();
            }

            foreach (var lyric in lyrics)
            {
                KaraokeLyrics.Add(lyric);
            }
        }

        private List<TimedTextElement> GetHallelujahLyrics()
        {
            return new List<TimedTextElement>
            {
                new("I've ", 0),
                new("heard ", 800),
                new("there ", 1400),
                new("was ", 1900),
                new("a ", 2300),
                new("._.", 2600),
                new("secret ", 2600),
                new("chord. ", 3400),
                new("That ", 4400),
                new("David ", 5000),
                new("played ", 5800),
                new("and ", 6400),
                new("._.", 6800),
                new("it ", 6800),
                new("pleased ", 7100),
                new("the ", 7900),
                new("Lord ", 8300),
                new("But ", 9500),
                new("you ", 10100),
                new("don't ", 10600),
                new("really ", 11300),
                new("care ", 12100),
                new("for ", 12700),
                new("music, ", 13100),
                new("do ", 13900),
                new("you? ", 14400),
                new("Hal", 15400),
                new("le", 16200),
                new("lu", 16600),
                new("jah! ", 17200),
                new("Hal", 18400),
                new("le", 19200),
                new("lu", 19600),
                new("jah! ", 20200)
            };
        }

        private List<TimedTextElement> GetBohemianRhapsodyLyrics()
        {
            return new List<TimedTextElement>
            {
                new("Is ", 0),
                new("this ", 400),
                new("the ", 900),
                new("real ", 1200),
                new("life? ", 1800),
                new("._.", 2600),
                new("Is ", 2600),
                new("this ", 3000),
                new("just ", 3500),
                new("fan", 4100),
                new("ta", 4500),
                new("sy? ", 4900),
                new("._.", 5700),
                new("Caught ", 5700),
                new("in ", 6300),
                new("a ", 6600),
                new("land", 6800),
                new("slide, ", 7300),
                new("._.", 8100),
                new("No ", 8100),
                new("es", 8500),
                new("cape ", 8800),
                new("from ", 9300),
                new("re", 9700),
                new("al", 10000),
                new("i", 10300),
                new("ty ", 10500)
            };
        }

        private List<TimedTextElement> GetHotelCaliforniaLyrics()
        {
            return new List<TimedTextElement>
            {
                new("Wel", 0),
                new("come ", 400),
                new("to ", 1000),
                new("the ", 1300),
                new("Ho", 1600),
                new("tel ", 2100),
                new("Cal", 2800),
                new("i", 3200),
                new("for", 3500),
                new("nia ", 3900),
                new("Such ", 4700),
                new("a ", 5200),
                new("love", 5500),
                new("ly ", 6000),
                new("place ", 6400),
                new("Such ", 7400),
                new("a ", 7900),
                new("love", 8200),
                new("ly ", 8700),
                new("face ", 9100)
            };
        }

        private async Task<List<TimedTextElement>> GetDefaultLyrics()
        {
            IReadOnlyList<IStorageFile> result = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                Title = "Select audio file",
                AllowMultiple = true,
                FileTypeFilter = new FilePickerFileType[] { _lyricFiles }
            });

            if(result.Count > 0)
            {
                if (result[0].TryGetLocalPath() != null)
                    return OwnKaraoke.Lyricfile.OwnKaraokeLyric.ParseFromFile(result[0].TryGetLocalPath()).ToList();
                else
                    return new List<TimedTextElement>();
            }
            else
            {
                return new List<TimedTextElement>();
            }
        }

        private void UpdateStatus(object? sender, EventArgs e)
        {
            if (_isPlaying)
            {
                var elapsed = DateTime.Now - _startTime;

                // Teljes időtartam kiszámítása: utolsó elem időpontja + annak időtartama
                if (KaraokeLyrics.Count > 0)
                {
                    var lastElement = KaraokeLyrics.Last();
                    double lastElementDuration = 2000; // Default időtartam az utolsó elemhez

                    // Ha van még egy elem utána, számítsuk ki az időtartamát
                    if (KaraokeLyrics.Count > 1)
                    {
                        var secondLastElement = KaraokeLyrics[KaraokeLyrics.Count - 2];
                        double timeDiff = lastElement.StartTimeMs - secondLastElement.StartTimeMs;
                        lastElementDuration = Math.Min(timeDiff * 0.75, 1000);
                    }

                    var totalDuration = lastElement.StartTimeMs + lastElementDuration;
                    var progress = Math.Min(100, (elapsed.TotalMilliseconds / totalDuration) * 100);

                    ProgressText.Text = $"Progress: {progress:F1}% | Time: {elapsed:mm\\:ss}";

                    if (progress >= 100)
                    {
                        StopButton_Click( sender, new RoutedEventArgs());
                        StatusText.Text = "Song complete! 🎉";
                    }
                }
            }
        }

        public event PropertyChangedEventHandler? Property_Changed;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            Property_Changed?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected override void OnClosed(EventArgs e)
        {
            _statusTimer?.Stop();
            base.OnClosed(e);
        }

        private void OnTempoSliderChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == RangeBase.ValueProperty && e.NewValue is double newTempo)
            {
                UpdateTempoPercentage(newTempo);
            }
        }

        private void UpdateTempoPercentage(double tempo)
        {
            var percentage = tempo * 100;
            var sign = percentage >= 0 ? "+" : "";
            TempoPercentage.Text = $"({sign}{percentage:F0}%)";
        }
    }
}