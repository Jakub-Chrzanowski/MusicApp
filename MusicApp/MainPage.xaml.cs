using MusicApp.Services;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace MusicApp
{
    public partial class MainPage : ContentPage
    {
        private ObservableCollection<SongModel> _songs = new();
        private readonly string _songsFilePath = Path.Combine(FileSystem.AppDataDirectory, "songs.json");
        private int _currentIndex = -1;
        public MainPage()
        {
            InitializeComponent();
            SongsCollectionView.ItemsSource = _songs;
        }

        private async void AddSongButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                var result = await FilePicker.PickMultipleAsync(new PickOptions
                {
                    PickerTitle = "Wybierz pliki muzyczne",
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.WinUI, new[] { ".mp3", ".wav", ".flac" } },
                        { DevicePlatform.Android, new[] { "audio/*" } },
                        { DevicePlatform.iOS, new[] { "public.audio" } }
                    })
                });

                if (result != null)
                {
                    foreach (var file in result)
                    {
                        _songs.Add(new SongModel
                        {
                            Title = System.IO.Path.GetFileNameWithoutExtension(file.FileName),
                            Path = file.FullPath
                        });
                    }
                }
                await SaveSongsAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Błąd", $"Nie udało się wybrać plików: {ex.Message}", "OK");
            }
        }
        private async Task SaveSongsAsync()
        {
            try
            {
                var json = JsonSerializer.Serialize(_songs);
                await File.WriteAllTextAsync(_songsFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd zapisu piosenek: {ex.Message}");
            }
        }
    }
}
