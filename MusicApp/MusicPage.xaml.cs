using MusicApp.Services;
using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Views;

namespace MusicApp;

public partial class MusicPage : ContentPage
{
    private ObservableCollection<SongModel> _songs = new();
    private int _currentIndex;
    private bool _isPlaying = false;
    private bool _isSeeking = false;
    private readonly string[] _artworks = new[] { "random_cover1.png", "random_cover2.png", "random_cover3.png" };
    private readonly Random _rnd = new();

    
    public MusicPage()
    {
        InitializeComponent();
        _songs = new ObservableCollection<SongModel>();
        SetupTimer();
    }

   
    public MusicPage(ObservableCollection<SongModel> songs, int startIndex = 0) : this()
    {
        InitializeWithSongs(songs, startIndex);
    }

   
    public void InitializeWithSongs(ObservableCollection<SongModel> songs, int startIndex = 0)
    {
        _songs = songs ?? new ObservableCollection<SongModel>();
        _currentIndex = Math.Clamp(startIndex, 0, Math.Max(0, _songs.Count - 1));

        if (_songs.Count > 0)
        {
            PlayCurrentSong();
        }
    }

    private void SetupTimer()
    {

        Dispatcher.StartTimer(TimeSpan.FromMilliseconds(500), () =>
        {
            try
            {
                if (!_isPlaying || Player == null) return true;

                if (Player.Duration is TimeSpan dur && dur.TotalSeconds > 0)
                {
                    var pos = Player.Position;
                    ProgressSlider.Minimum = 0;
                    ProgressSlider.Maximum = dur.TotalSeconds;
                    if (!_isSeeking)
                        ProgressSlider.Value = pos.TotalSeconds;

                    TimeLabel.Text = $"{FormatTime(pos)} / {FormatTime(dur)}";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
            return true;
        });
    }

    private void PlayCurrentSong()
    {
        if (_songs == null || _songs.Count == 0 || _currentIndex < 0 || _currentIndex >= _songs.Count) return;

        var song = _songs[_currentIndex];

        try
        {
            Player.Source = MediaSource.FromFile(song.Path);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Nie można ustawić źródła: {ex.Message}");
            Player.Source = null;
        }

        try
        {
            Player.Play();
            _isPlaying = true;
            CurrentSongLabel.Text = $"▶ {song.Title}";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Błąd odtwarzania: {ex.Message}");
            _isPlaying = false;
            CurrentSongLabel.Text = $"Błąd odtwarzania: {song.Title}";
        }

        var art = _artworks[_rnd.Next(0, _artworks.Length)];
        ArtworkImage.Source = art;
        stopAndplay_btn.Source = "stop.png";
    }

    private void previous_btn_Clicked(object sender, EventArgs e)
    {
        if (_songs == null || _songs.Count == 0) return;
        _currentIndex--;
        if (_currentIndex < 0) _currentIndex = _songs.Count - 1;
        PlayCurrentSong();
    }

    private void another_btn_Clicked(object sender, EventArgs e)
    {
        if (_songs == null || _songs.Count == 0) return;
        _currentIndex = (_currentIndex + 1) % _songs.Count;
        PlayCurrentSong();
    }

    private void stopAndplay_btn_Clicked(object sender, EventArgs e)
    {
        if (_songs == null || _songs.Count == 0) return;

        if (_isPlaying)
        {
            try { Player.Pause(); } catch { }
            _isPlaying = false;
            CurrentSongLabel.Text = $"⏸ {_songs[_currentIndex].Title}";
            stopAndplay_btn.Source = "play.png";
        }
        else
        {
            if (Player.Source == null)
                PlayCurrentSong();
            else
                Player.Play();

            _isPlaying = true;
            CurrentSongLabel.Text = $"▶ {_songs[_currentIndex].Title}";
            stopAndplay_btn.Source = "stop.png";
        }
    }

    private void Player_MediaOpened(object sender, EventArgs e)
    {
        if (Player.Duration is TimeSpan dur && dur.TotalSeconds > 0)
        {
            ProgressSlider.Minimum = 0;
            ProgressSlider.Maximum = dur.TotalSeconds;
            ProgressSlider.Value = Player.Position.TotalSeconds;
            TimeLabel.Text = $"{FormatTime(Player.Position)} / {FormatTime(dur)}";
        }
    }

    private void Player_MediaEnded(object sender, EventArgs e)
    {
        another_btn_Clicked(null, EventArgs.Empty);
    }

    private void ProgressSlider_DragStarted(object sender, EventArgs e)
    {
        _isSeeking = true;
    }

    private void ProgressSlider_DragCompleted(object sender, EventArgs e)
    {
        try
        {
            if (Player.Duration is TimeSpan dur && dur.TotalSeconds > 0)
            {
                var seconds = ProgressSlider.Value;
                Player.SeekTo(TimeSpan.FromSeconds(seconds));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
        finally
        {
            _isSeeking = false;
        }
    }

    private string FormatTime(TimeSpan ts) => ts.ToString(@"mm\:ss");
}