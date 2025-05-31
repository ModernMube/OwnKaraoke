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
    /// <summary>
    /// Main window class for the karaoke application that implements property change notifications.
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Collection of timed text elements representing the karaoke lyrics.
        /// </summary>
        private ObservableCollection<TimedTextElement> _karaokeLyrics = new();
        
        /// <summary>
        /// Timer for updating UI elements at regular intervals.
        /// </summary>
        private DispatcherTimer? _statusTimer;
        
        /// <summary>
        /// Subscription for monitoring karaoke status changes.
        /// </summary>
        private IDisposable? _statusSubscription;
        
        /// <summary>
        /// Subscription for monitoring position changes in the karaoke playback.
        /// </summary>
        private IDisposable? _positionSubscription;
        
        /// <summary>
        /// Subscription for monitoring duration changes in the karaoke playback.
        /// </summary>
        private IDisposable? _durationSubscription;

        /// <summary>
        /// Gets or sets the collection of timed text elements representing the karaoke lyrics.
        /// </summary>
        public ObservableCollection<TimedTextElement> KaraokeLyrics
        {
            get => _karaokeLyrics;
            set
            {
                _karaokeLyrics = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            InitializeSongs();
            SetupStatusTimer();
            SetupKaraokeSubscriptions();

            // FIX: Proper tempo slider event handling
            SetupTempoSlider();

            ProgressBar.PointerPressed += ProgressBar_PointerPressed;

            UpdateTempoPercentage(0.0);
        }

        /// <summary>
        /// Sets up the tempo slider event handling.
        /// </summary>
        private void SetupTempoSlider()
        {
            // Initialization: slider value = control tempo value
            //TempoSlider.Value = KaraokeControl.Tempo;

            // Event handling: when the slider changes, update the control's tempo
            TempoSlider.GetObservable(RangeBase.ValueProperty)
                .Subscribe(newValue =>
                {
                    if (newValue is double tempo)
                    {
                        // MAIN TASK: Setting the tempo property of the karaoke control
                        KaraokeControl.Tempo = tempo;

                        // UI update
                        UpdateTempoPercentage(tempo);
                    }
                });
        }

        /// <summary>
        /// Initializes the list of available songs in the song selector.
        /// </summary>
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

        /// <summary>
        /// Sets up the timer for regular UI updates.
        /// </summary>
        private void SetupStatusTimer()
        {
            _statusTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _statusTimer.Tick += UpdateUI;
            _statusTimer.Start();
        }

        /// <summary>
        /// Sets up subscriptions to monitor karaoke control property changes.
        /// </summary>
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

        /// <summary>
        /// Handles karaoke status changes and updates the UI accordingly.
        /// </summary>
        /// <param name="status">The new karaoke status.</param>
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

        /// <summary>
        /// Handles position changes in the karaoke playback and updates the UI.
        /// </summary>
        /// <param name="position">The new position in milliseconds.</param>
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

        /// <summary>
        /// Handles duration changes in the karaoke playback and updates the UI.
        /// </summary>
        /// <param name="duration">The new duration in milliseconds.</param>
        private void OnDurationChanged(double duration)
        {
            Dispatcher.UIThread.Post(() =>
            {
                var timeSpan = TimeSpan.FromMilliseconds(duration);
                TotalTimeText.Text = $"{timeSpan:mm\\:ss}";
                ProgressBar.Maximum = 100;
            });
        }

        /// <summary>
        /// Updates the control states based on the current karaoke status.
        /// </summary>
        /// <param name="status">The current karaoke status.</param>
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

        /// <summary>
        /// Handles the click event for the play/pause button.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
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

        /// <summary>
        /// Handles the click event for the stop button.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            KaraokeControl.Stop();
        }

        /// <summary>
        /// Handles the click event for the reset button.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
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

        /// <summary>
        /// Handles the click event for the tempo reset button.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ResetTempoButton_Click(object sender, RoutedEventArgs e)
        {
            TempoSlider.Value = 0.0; // This automatically sets KaraokeControl.Tempo as well
        }

        /// <summary>
        /// Handles the click event for the faster tempo button.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void FasterButton_Click(object sender, RoutedEventArgs e)
        {
            var newTempo = Math.Min(2.0, TempoSlider.Value + 0.1);
            TempoSlider.Value = newTempo;
        }

        /// <summary>
        /// Handles the click event for the slower tempo button.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void SlowerButton_Click(object sender, RoutedEventArgs e)
        {
            var newTempo = Math.Max(-2.0, TempoSlider.Value - 0.1);
            TempoSlider.Value = newTempo;
        }

        /// <summary>
        /// Handles the pointer pressed event for the progress bar to seek within the song.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
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

        /// <summary>
        /// Handles the selection changed event for the song selector.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
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

        /// <summary>
        /// Loads the lyrics for the selected song.
        /// </summary>
        /// <param name="songName">The name of the song to load.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Gets the lyrics for "Hallelujah" by Leonard Cohen.
        /// </summary>
        /// <returns>A list of timed text elements representing the lyrics.</returns>
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

        /// <summary>
        /// Gets the lyrics for "Bohemian Rhapsody" by Queen.
        /// </summary>
        /// <returns>A list of timed text elements representing the lyrics.</returns>
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

        /// <summary>
        /// Gets the lyrics for "Hotel California" by Eagles.
        /// </summary>
        /// <returns>A list of timed text elements representing the lyrics.</returns>
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

        /// <summary>
        /// Gets lyrics from a file selected by the user.
        /// </summary>
        /// <returns>A list of timed text elements representing the lyrics.</returns>
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

        /// <summary>
        /// Updates the UI with current karaoke information at regular intervals.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
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

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Called when the window is closed to clean up resources.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnClosed(EventArgs e)
        {
            _statusTimer?.Stop();
            _statusSubscription?.Dispose();
            _positionSubscription?.Dispose();
            _durationSubscription?.Dispose();
            base.OnClosed(e);
        }

        /// <summary>
        /// REMOVED: The old OnTempoSliderChanged method, as it's no longer needed.
        /// The new SetupTempoSlider() method handles this.
        /// </summary>

        /// <summary>
        /// Updates the displayed tempo percentage based on the current tempo value.
        /// </summary>
        /// <param name="tempo">The current tempo value.</param>
        private void UpdateTempoPercentage(double tempo)
        {
            var percentage = tempo * 100;
            var sign = percentage >= 0 ? "+" : "";
            TempoPercentage.Text = $"({sign}{percentage:F0}%)";
        }
    }
}