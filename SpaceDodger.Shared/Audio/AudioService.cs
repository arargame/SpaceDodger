using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using SpaceDodger.Persistence;

namespace SpaceDodger.Audio
{
    public sealed class TrackInfo
    {
        public string Title { get; }
        public string FileName { get; }

        public TrackInfo(string title, string fileName)
        {
            Title = title;
            FileName = fileName;
        }
    }

    /// <summary>
    /// Unified audio service managing sound effects and non-repeating shuffled music playback.
    /// </summary>
    public sealed class AudioService : IDisposable
    {
        private static readonly TrackInfo[] AllTracks =
        {
            new TrackInfo("Arcade Pulse", "Arcade Pulse.ogg"),
            new TrackInfo("Coin Dash Circuit", "Coin Dash Circuit.ogg"),
            new TrackInfo("Coin Drop Sprint", "Coin Drop Sprint.ogg"),
            new TrackInfo("Coin-Op Mirage", "Coin-Op Mirage.ogg"),
            new TrackInfo("Pixel Saloon", "Pixel Saloon.ogg"),
        };

        private readonly Dictionary<string, SoundEffect> _effects = new Dictionary<string, SoundEffect>();
        private readonly Dictionary<string, Song> _cachedSongs = new Dictionary<string, Song>(StringComparer.OrdinalIgnoreCase);
        private readonly List<int> _playlist = new List<int>();
        private readonly Random _random = new Random();
        private readonly SaveData _settings;

        private int _playlistIndex = -1;
        private int _lastTrackIndex = -1;

        public TrackInfo CurrentTrack =>
            _lastTrackIndex >= 0 && _lastTrackIndex < AllTracks.Length ? AllTracks[_lastTrackIndex] : null;

        public AudioService(SaveData settings)
        {
            _settings = settings;
            try
            {
                MediaPlayer.IsRepeating = false;
                MediaPlayer.Volume = 0.60f;
            }
            catch { }
        }

        public void Play(string name, float volume)
        {
            if (!_settings.SoundEnabled) return;
            try
            {
                if (!_effects.TryGetValue(name, out var effect))
                {
                    using var stream = TitleContainer.OpenStream($"Content/sounds/{name}.wav");
                    effect = SoundEffect.FromStream(stream);
                    _effects[name] = effect;
                }
                effect.Play(MathHelper.Clamp(volume, 0f, 1f), 0f, 0f);
            }
            catch (Exception) { }
        }

        public void OnMusicSettingChanged(bool enabled)
        {
            if (!enabled)
            {
                try
                {
                    if (MediaPlayer.State == MediaState.Playing)
                        MediaPlayer.Pause();
                }
                catch { }
            }
            else
            {
                try
                {
                    if (MediaPlayer.State == MediaState.Paused)
                    {
                        MediaPlayer.Resume();
                        if (MediaPlayer.State != MediaState.Playing)
                            PlayNextTrack();
                    }
                    else if (MediaPlayer.State == MediaState.Stopped)
                    {
                        PlayNextTrack();
                    }
                }
                catch
                {
                    PlayNextTrack();
                }
            }
        }

        public void Update(float dt)
        {
            if (!_settings.MusicEnabled)
            {
                if (MediaPlayer.State == MediaState.Playing)
                {
                    try { MediaPlayer.Pause(); } catch { }
                }
                return;
            }

            if (MediaPlayer.State == MediaState.Paused)
            {
                try
                {
                    MediaPlayer.Resume();
                    if (MediaPlayer.State != MediaState.Playing)
                        PlayNextTrack();
                }
                catch
                {
                    PlayNextTrack();
                }
                return;
            }

            if (MediaPlayer.State == MediaState.Stopped)
            {
                PlayNextTrack();
            }
        }

        public void PlayNextTrack()
        {
            if (!_settings.MusicEnabled || AllTracks.Length == 0)
                return;

            if (_playlist.Count == 0 || _playlistIndex >= _playlist.Count)
            {
                ReshufflePlaylist();
            }

            int trackIdx = _playlist[_playlistIndex++];
            _lastTrackIndex = trackIdx;
            var track = AllTracks[trackIdx];

            try
            {
                if (!_cachedSongs.TryGetValue(track.FileName, out var song))
                {
                    song = LoadSong(track.Title, track.FileName);
                    _cachedSongs[track.FileName] = song;
                }

                MediaPlayer.IsRepeating = false;
                MediaPlayer.Play(song);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AudioService] Error playing track '{track.FileName}': {ex.Message}");
                // If loading failed, advance to next track in queue
                if (_playlistIndex < _playlist.Count)
                    PlayNextTrack();
            }
        }

        private Song LoadSong(string title, string fileName)
        {
            string relativePath = $"Content/musics/{fileName}";
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "musics", fileName);
            Uri uri = File.Exists(fullPath)
                ? new Uri(fullPath, UriKind.Absolute)
                : new Uri(relativePath, UriKind.Relative);

            return Song.FromUri(title, uri);
        }

        private void ReshufflePlaylist()
        {
            _playlist.Clear();
            for (int i = 0; i < AllTracks.Length; i++)
                _playlist.Add(i);

            // Fisher-Yates shuffle
            for (int i = _playlist.Count - 1; i > 0; i--)
            {
                int swapIndex = _random.Next(i + 1);
                int temp = _playlist[i];
                _playlist[i] = _playlist[swapIndex];
                _playlist[swapIndex] = temp;
            }

            // Ensure first track of new cycle is never the same as last track of previous cycle
            if (_playlist.Count > 1 && _playlist[0] == _lastTrackIndex)
            {
                int swapIndex = _random.Next(1, _playlist.Count);
                int temp = _playlist[0];
                _playlist[0] = _playlist[swapIndex];
                _playlist[swapIndex] = temp;
            }

            _playlistIndex = 0;
        }

        public void Dispose()
        {
            try { MediaPlayer.Stop(); } catch { }
            foreach (var song in _cachedSongs.Values)
            {
                try { song.Dispose(); } catch { }
            }
            _cachedSongs.Clear();

            foreach (var effect in _effects.Values)
            {
                try { effect.Dispose(); } catch { }
            }
            _effects.Clear();
        }
    }
}
