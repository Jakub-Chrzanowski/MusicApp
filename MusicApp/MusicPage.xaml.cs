using MusicApp.Services;
using System.Collections.ObjectModel;


namespace MusicApp;

public partial class MusicPage : ContentPage
{
    private ObservableCollection<SongModel> _songs;

    public MusicPage()
	{
		InitializeComponent();
        // _songs = songs;
	}

    private void stopAndplay_btn_Clicked(object sender, EventArgs e)
    {
        if (_songs.Count == 0) return;
        /*if (_currentIndex == -1) _currentIndex = 0;

        Player.Source = _songs[_currentIndex].Path; // ustaw Source zawsze
        Player.Play();
        CurrentSongLabel.Text = $"▶ {_songs[_currentIndex].Title}";
        */
    }
}