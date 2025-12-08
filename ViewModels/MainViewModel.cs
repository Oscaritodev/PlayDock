using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using NewLauncher.Helpers;
using NewLauncher.Models;
using NewLauncher.Views; // Reference Views namespace

namespace NewLauncher.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private GameItem? _selectedGame;
        private string _currentTime = DateTime.Now.ToString("hh:mm tt");

        public ObservableCollection<GameItem> Games { get; set; }

        public ICommand AddGameCommand { get; }
        public ICommand PlayGameCommand { get; }
        public ICommand EditGameCommand { get; }

        public string CurrentTime
        {
            get => _currentTime;
            set
            {
                _currentTime = value;
                OnPropertyChanged();
            }
        }

        public GameItem? SelectedGame
        {
            get => _selectedGame;
            set
            {
                _selectedGame = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            Games = new ObservableCollection<GameItem>();
            AddGameCommand = new RelayCommand(ExecuteAddGame);
            PlayGameCommand = new RelayCommand(ExecutePlayGame, CanExecutePlayGame);
            EditGameCommand = new RelayCommand(ExecuteEditGame, CanExecutePlayGame);
            
            LoadGames();
            StartClock();
        }

        private void StartClock()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) => CurrentTime = DateTime.Now.ToString("hh:mm tt");
            timer.Start();
        }

        private void ExecutePlayGame(object? parameter)
        {
            var game = parameter as GameItem ?? SelectedGame;
            if (game != null && File.Exists(game.ExePath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = game.ExePath,
                        WorkingDirectory = Path.GetDirectoryName(game.ExePath),
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error launching game: {ex.Message}");
                }
            }
        }

        private void ExecuteEditGame(object? parameter)
        {
            // parameter might be passed from CommandParameter, otherwise use Selected
            var game = parameter as GameItem ?? SelectedGame;
            if (game != null)
            {
                // We need to resolve the Window to set Owner, but MVVM purists might object.
                // For simplicity here, we instantiate directly.
                var dialog = new EditGameDialog(game);
                if (Application.Current.MainWindow != null)
                    dialog.Owner = Application.Current.MainWindow;

                if (dialog.ShowDialog() == true)
                {
                    // Update Model
                    game.Title = dialog.GameTitle;
                    game.Description = dialog.GameDescription;
                    
                    // Force UI update (since properties might not raise INotifyPropertyChanged in GameItem if simple POCO)
                    // But GameItem defaults usually don't implement INPC. Let's assume we need to refresh list or GameItem needs INPC.
                    // Ideally GameItem should implement INotifyPropertyChanged.
                    // For now, refreshing selection triggers update in some views, but title in list might not update.
                    // Let's rely on standard binding if GameItem is static, it won't update.
                    // We will just replace the item in the list to force update.
                    int index = Games.IndexOf(game);
                    if (index != -1)
                    {
                        Games.RemoveAt(index);
                        Games.Insert(index, game);
                        SelectedGame = game;
                    }
                }
            }
        }

        private bool CanExecutePlayGame(object? parameter)
        {
            return SelectedGame != null || parameter != null;
        }

        private void ExecuteAddGame(object? parameter)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Executables (*.exe)|*.exe|All files (*.*)|*.*",
                Title = "Select Game Executable"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                AddGame(filePath);
                SelectedGame = Games[Games.Count - 1];
            }
        }

        private void LoadGames()
        {
            // Initial mock data
        }

        private void AddGame(string path)
        {
            if (File.Exists(path))
            {
                var icon = IconExtractor.Extract(path);
                
                string title = Path.GetFileNameWithoutExtension(path);
                string description = "No description available.";

                try 
                {
                    var versionInfo = FileVersionInfo.GetVersionInfo(path);
                    if (!string.IsNullOrWhiteSpace(versionInfo.FileDescription))
                        title = versionInfo.FileDescription;
                     if (!string.IsNullOrWhiteSpace(versionInfo.ProductName))
                        description = versionInfo.ProductName;
                }
                catch { }

                Games.Add(new GameItem
                {
                    Title = title,
                    ExePath = path,
                    Icon = icon,
                    PlayTime = "0h",
                    LastPlayed = DateTime.Now,
                    BackgroundImage = icon,
                    Description = description
                });
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
