using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using OwnKaraoke;
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
        private IDisposable? _statusSubscription;
        private IDisposable? _positionSubscription;
        private IDisposable? _durationSubscription;

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
            SetupKaraokeSubscriptions();

            TempoSlider.PropertyChanged += OnTempoSliderChanged;
            ProgressBar.PointerPressed += ProgressBar_PointerPressed;

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
            _statusTimer.Tick += UpdateUI;
            _statusTimer.Start();
        }

        private void SetupKaraokeSubscriptions()
        {
            // Subscribe to status changes
            _statusSubscription = KaraokeControl.GetObservable(OwnKaraokeDisplay.StatusProperty)
                .Subscribe(OnKaraokeStatusChanged);

            // Subscribe to position changes
            _positionSubscription = KaraokeControl.GetObservable(OwnKaraokeDisplay.PositionProperty)
                .Subscribe(OnPositionChanged);

            // Subscribe to duration changes
            _durationSubscription = KaraokeControl.GetObservable(OwnKaraokeDisplay.DurationProperty)
                .Subscribe(OnDurationChanged);
        }

        private void OnKaraokeStatusChanged(KaraokeStatus status)
        {
            Dispatcher.UIThread.Post(() =>
            {
                UpdateControlsForStatus(status);
                StatusText.Text = status switch
                {
                    KaraokeStatus.Idle => "Ready to launch",
                    KaraokeStatus.Playing => "Playing...",
                    KaraokeStatus.Paused => "Paused",
                    KaraokeStatus.Finished => "Song finished! 🎉",
                    _ => "Unknown status"
                };
            });
        }

        private void OnPositionChanged(double position)
        {
            Dispatcher.UIThread.Post(() =>
            {
                var timeSpan = TimeSpan.FromMilliseconds(position);
                CurrentTimeText.Text = $"{timeSpan:mm\\:ss}";

                if (KaraokeControl.Duration > 0)
                {
                    ProgressBar.Value = (position / KaraokeControl.Duration) * 100;
                }
            });
        }

        private void OnDurationChanged(double duration)
        {
            Dispatcher.UIThread.Post(() =>
            {
                var timeSpan = TimeSpan.FromMilliseconds(duration);
                TotalTimeText.Text = $"{timeSpan:mm\\:ss}";
                ProgressBar.Maximum = 100;
            });
        }

        private void UpdateControlsForStatus(KaraokeStatus status)
        {
            switch (status)
            {
                case KaraokeStatus.Idle:
                    PlayPauseButton.Content = "▶️ Play";
                    PlayPauseButton.Background = Avalonia.Media.Brushes.Green;
                    PlayPauseButton.IsEnabled = KaraokeLyrics.Any();
                    StopButton.IsEnabled = false;
                    break;

                case KaraokeStatus.Playing:
                    PlayPauseButton.Content = "⏸️ Pause";
                    PlayPauseButton.Background = Avalonia.Media.Brushes.Orange;
                    PlayPauseButton.IsEnabled = true;
                    StopButton.IsEnabled = true;
                    break;

                case KaraokeStatus.Paused:
                    PlayPauseButton.Content = "▶️ Resume";
                    PlayPauseButton.Background = Avalonia.Media.Brushes.Green;
                    PlayPauseButton.IsEnabled = true;
                    StopButton.IsEnabled = true;
                    break;

                case KaraokeStatus.Finished:
                    PlayPauseButton.Content = "▶️ Play";
                    PlayPauseButton.Background = Avalonia.Media.Brushes.Green;
                    PlayPauseButton.IsEnabled = true;
                    StopButton.IsEnabled = false;
                    break;
            }
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (!KaraokeLyrics.Any())
            {
                StatusText.Text = "Please choose a song!";
                return;
            }

            switch (KaraokeControl.Status)
            {
                case KaraokeStatus.Idle:
                case KaraokeStatus.Finished:
                    KaraokeControl.Start();
                    break;

                case KaraokeStatus.Playing:
                    KaraokeControl.Pause();
                    break;

                case KaraokeStatus.Paused:
                    KaraokeControl.Resume();
                    break;
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            KaraokeControl.Stop();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            KaraokeControl.Stop();

            if (KaraokeLyrics.Any())
            {
                var currentSong = SongSelector.SelectedItem?.ToString();
                if (currentSong is not null)
                {
                    _ = LoadSongLyrics(currentSong);
                }
            }
        }

        private void ProgressBar_PointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (KaraokeControl.Duration > 0 && sender is ProgressBar progressBar)
            {
                var position = e.GetPosition(progressBar);
                var percentage = position.X / progressBar.Bounds.Width;
                var targetPosition = percentage * KaraokeControl.OriginalDuration;

                KaraokeControl.Seek(targetPosition);
            }
        }

        private void SongSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SongSelector.SelectedItem is string selectedSong)
            {
                _ = LoadSongLyrics(selectedSong);
                StatusText.Text = $"Song loaded: {selectedSong}";

                PlayPauseButton.IsEnabled = true;
                StopButton.IsEnabled = true;

                if (KaraokeControl.Status == KaraokeStatus.Playing)
                {
                    KaraokeControl.Stop();
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
                Title = "Select lyric file",
                AllowMultiple = false,
                FileTypeFilter = new FilePickerFileType[] { _lyricFiles }
            });

            if (result.Count > 0)
            {
                var localPath = result[0].TryGetLocalPath();
                if (localPath != null)
                    return OwnKaraoke.Lyricfile.OwnKaraokeLyric.ParseFromFile(localPath).ToList();
            }

            return new List<TimedTextElement>();
        }

        private void UpdateUI(object? sender, EventArgs e)
        {
            // Update debug information
            if (KaraokeControl.Status != KaraokeStatus.Idle)
            {
                var currentTempo = KaraokeControl.Tempo;
                var tempoPercentage = currentTempo * 100;
                var sign = tempoPercentage >= 0 ? "+" : "";

                var debugInfo = $"Status: {KaraokeControl.Status}";
                if (Math.Abs(currentTempo) > 0.01)
                {
                    debugInfo += $" | Tempo: {sign}{tempoPercentage:F0}%";
                }

                debugInfo += $" | Pos: {KaraokeControl.Position:F0}ms/{KaraokeControl.Duration:F0}ms";
                debugInfo += $" | Orig: {KaraokeControl.OriginalPosition:F0}ms/{KaraokeControl.OriginalDuration:F0}ms";

                DebugInfoText.Text = debugInfo;
            }
            else
            {
                DebugInfoText.Text = "";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected override void OnClosed(EventArgs e)
        {
            _statusTimer?.Stop();
            _statusSubscription?.Dispose();
            _positionSubscription?.Dispose();
            _durationSubscription?.Dispose();
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