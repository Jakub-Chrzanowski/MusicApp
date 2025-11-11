using MusicApp.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace MusicApp
{
    public partial class MainPage : ContentPage
    {
        private ObservableCollection<SongModel> _songs = new();
        private readonly string _songsFilePath = Path.Combine(FileSystem.AppDataDirectory, "songs.json");

    
        private SongModel? _selectedSong;

        public MainPage()
        {
            InitializeComponent();
            SongsCollectionView.ItemsSource = _songs;
            LoadSongsAsync().FireAndForget();
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

                    await SaveSongsAsync();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Błąd", $"Nie udało się wybrać plików: {ex.Message}", "OK");
            }
        }

     
        private async void Song_Tapped(object sender, EventArgs e)
        {
            try
            {
                if (sender is VisualElement ve && ve.BindingContext is SongModel song)
                {
                    
                    _selectedSong = song;

                    SongsCollectionView.SelectedItem = song;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        
        private async void SongsCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection?.FirstOrDefault() is SongModel selected)
            {
                var index = _songs.IndexOf(selected);
                if (index >= 0)
                {
           
                    _selectedSong = selected;
                    await Navigation.PushAsync(new MusicPage(_songs, index));
                }

                if (sender is CollectionView cv)
                    cv.SelectedItem = null;
            }
        }

        private async void SaveSongsToFile_Clicked(object sender, EventArgs e)
        {
            try
            {
                await SaveSongsAsync();
                await DisplayAlert("Zapisano", "Lista piosenek została zapisana do pliku.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Błąd zapisu", ex.Message, "OK");
            }
        }


        private async void DeleteSongButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                SongModel? toRemove = _selectedSong;

            
                if (toRemove == null && SongsCollectionView.SelectedItem is SongModel sel)
                    toRemove = sel;

                if (toRemove == null)
                {
                    await DisplayAlert("Usuń", "Nie wybrano utworu do usunięcia.", "OK");
                    return;
                }

                bool confirmRemove = await DisplayAlert("Usuń", $"Usunąć \"{toRemove.Title}\" z playlisty?", "Tak", "Nie");
                if (!confirmRemove) return;


                if (!string.IsNullOrWhiteSpace(toRemove.Path) && File.Exists(toRemove.Path))
                {
                    bool deleteFile = await DisplayAlert("Usuń plik", "Plik audio został znaleziony na dysku. Usunąć również plik z dysku?", "Tak", "Nie");
                    if (deleteFile)
                    {
                        try
                        {
                            File.Delete(toRemove.Path);
                        }
                        catch (Exception exDel)
                        {
                           
                            await DisplayAlert("Błąd", $"Nie udało się usunąć pliku: {exDel.Message}", "OK");
                        }
                    }
                }

       
                _songs.Remove(toRemove);
                await SaveSongsAsync();

        
                _selectedSong = null;
                SongsCollectionView.SelectedItem = null;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Błąd", ex.Message, "OK");
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
                throw;
            }
        }

        private async Task LoadSongsAsync()
        {
            try
            {
                if (File.Exists(_songsFilePath))
                {
                    var json = await File.ReadAllTextAsync(_songsFilePath);
                    var loaded = JsonSerializer.Deserialize<ObservableCollection<SongModel>>(json);
                    if (loaded != null)
                    {
                        foreach (var s in loaded)
                            _songs.Add(s);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd odczytu piosenek: {ex.Message}");
            }
        }
    }

    static class TaskExtensions
    {
        public static async void FireAndForget(this Task task)
        {
            try { await task; } catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex); }
        }
    }
}