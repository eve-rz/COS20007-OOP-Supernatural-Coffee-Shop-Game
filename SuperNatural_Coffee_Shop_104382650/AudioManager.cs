using Raylib_cs;
using System.Collections.Generic;
using System.IO;
using System;

namespace CoffeeShop
{
    /// <summary>
    /// Manages loading, playing, stopping, and updating music streams for the game.
    /// </summary>
    public class AudioManager
    {
        /// <summary>
        /// Music stream for the main game song.
        /// </summary>
        private Music _gameSong;
        /// <summary>
        /// Music stream for the workstation screen song.
        /// </summary>
        private Music _workstationSong;
        /// <summary>
        /// Music stream for the level 4 specific song.
        /// </summary>
        private Music _level4Song;
        /// <summary>
        /// Music stream for the level 4 workstation specific song.
        /// </summary>
        private Music _level4WorkstationSong;

        /// <summary>
        /// The currently playing music stream.
        /// </summary>
        private Music _currentMusic;
        /// <summary>
        /// An identifier for the currently playing song to prevent restarting the same track unnecessarily.
        /// </summary>
        private string _currentSongNameIdentifier;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioManager"/> class.
        /// Loads all music tracks from the specified audio path.
        /// </summary>
        /// <param name="audioPath">The directory path where the audio files are located.</param>
        public AudioManager(string audioPath)
        {
            _currentMusic = new Music(); 
            _currentSongNameIdentifier = "";
            LoadAllMusic(audioPath);
        }

        /// <summary>
        /// Loads all predefined music streams from the specified directory.
        /// </summary>
        /// <param name="audioPath">The path to the directory containing the .ogg and mp3 audio files.</param>
        private void LoadAllMusic(string audioPath)
        {
            if (!Directory.Exists(audioPath))
            {
                Console.WriteLine($"WARNING: Audio directory not found at '{audioPath}'. Music will not load.");
                return;
            }

            _gameSong = Raylib.LoadMusicStream(Path.Combine(audioPath, "game_song.ogg"));
            _workstationSong = Raylib.LoadMusicStream(Path.Combine(audioPath, "workstation_screen_audio.mp3"));
            _level4Song = Raylib.LoadMusicStream(Path.Combine(audioPath, "level4_song.ogg"));
            _level4WorkstationSong = Raylib.LoadMusicStream(Path.Combine(audioPath, "level4_workstation.ogg"));

            LogMusicLoadStatus("game_song.ogg", _gameSong);
            LogMusicLoadStatus("workstation_screen_audio.mp3", _workstationSong);
            LogMusicLoadStatus("level4_song.ogg", _level4Song);
            LogMusicLoadStatus("level4_workstation.ogg", _level4WorkstationSong);
        }

        /// <summary>
        /// Logs the loading status of a specific music file.
        /// </summary>
        /// <param name="fileName">The name of the music file.</param>
        /// <param name="music">The <see cref="Music"/> object that was attempted to be loaded.</param>
        private void LogMusicLoadStatus(string fileName, Music music)
        {
            if (music.Stream.Buffer != IntPtr.Zero)
                Console.WriteLine($"SUCCESS: Music '{fileName}' loaded.");
            else
                Console.WriteLine($"WARNING: Failed to load music '{fileName}'.");
        }

        /// <summary>
        /// Plays the specified music stream in a loop. If another song is playing, it's stopped first.
        /// If the specified music stream is invalid, it stops any current music.
        /// </summary>
        /// <param name="musicToPlay">The <see cref="Music"/> stream to play.</param>
        /// <param name="songNameIdentifier">A string identifier for the song, used to prevent restarting the same song.</param>
        public void PlayMusicLooping(Music musicToPlay, string songNameIdentifier)
        {
            // Check if the provided music stream is valid (loaded correctly)
            if (musicToPlay.Stream.Buffer == IntPtr.Zero)
            {
                Console.WriteLine($"AudioManager: Attempted to play invalid music stream for identifier '{songNameIdentifier}'. Stopping current music.");
                StopCurrentMusic(); 
                return;
            }

            if (_currentSongNameIdentifier == songNameIdentifier && Raylib.IsMusicStreamPlaying(_currentMusic))
            {
                return;
            }

            StopCurrentMusic(); // Stop whatever is currently playing before starting a new one

            _currentMusic = musicToPlay;
            _currentSongNameIdentifier = songNameIdentifier;
            Raylib.PlayMusicStream(_currentMusic);
            Raylib.SetMusicVolume(_currentMusic, 0.5f); // Set a default volume
            Console.WriteLine($"AudioManager: Playing '{songNameIdentifier}'");
        }

        /// <summary>
        /// Stops the currently playing music stream.
        /// </summary>
        public void StopCurrentMusic()
        {
            if (_currentMusic.Stream.Buffer != IntPtr.Zero && Raylib.IsMusicStreamPlaying(_currentMusic))
            {
                Raylib.StopMusicStream(_currentMusic);
                Console.WriteLine($"AudioManager: Stopped '{_currentSongNameIdentifier}'");
            }
            _currentSongNameIdentifier = ""; 
        }

        /// <summary>
        /// Updates the music stream buffers. This should be called every frame if music is playing.
        /// Handles looping of the current song.
        /// </summary>
        public void UpdateMusicStreams()
        {
            // Ensure there's a valid, playing music stream
            if (_currentMusic.Stream.Buffer != IntPtr.Zero && Raylib.IsMusicStreamPlaying(_currentMusic))
            {
                Raylib.UpdateMusicStream(_currentMusic); // This is crucial for Raylib music streams

                // Simple looping mechanism: if the song is near its end, rewind it.
                // Raylib's UpdateMusicStream might handle some looping internally depending on format,
                // but explicit control is often better.
                if (Raylib.GetMusicTimePlayed(_currentMusic) >= Raylib.GetMusicTimeLength(_currentMusic) - 0.1f) 
                {
                    Raylib.SeekMusicStream(_currentMusic, 0); 
                    Console.WriteLine($"AudioManager: Looping '{_currentSongNameIdentifier}'");
                }
            }
        }

        /// <summary>
        /// Gets the loaded game song.
        /// </summary>
        /// <returns>The <see cref="Music"/> object for the game song.</returns>
        public Music GetGameSong()
        {
            return _gameSong;
        }

        /// <summary>
        /// Gets the loaded workstation song.
        /// </summary>
        /// <returns>The <see cref="Music"/> object for the workstation song.</returns>
        public Music GetWorkstationSong()
        {
            return _workstationSong;
        }

        /// <summary>
        /// Gets the loaded level 4 song.
        /// </summary>
        /// <returns>The <see cref="Music"/> object for the level 4 song.</returns>
        public Music GetLevel4Song()
        {
            return _level4Song;
        }

        /// <summary>
        /// Gets the loaded level 4 workstation song.
        /// </summary>
        /// <returns>The <see cref="Music"/> object for the level 4 workstation song.</returns>
        public Music GetLevel4WorkstationSong()
        {
            return _level4WorkstationSong;
        }

        /// <summary>
        /// Stops any currently playing music and unloads all loaded music streams to free resources.
        /// </summary>
        public void UnloadAllMusic()
        {
            StopCurrentMusic();
            if (_gameSong.Stream.Buffer != IntPtr.Zero) Raylib.UnloadMusicStream(_gameSong);
            if (_workstationSong.Stream.Buffer != IntPtr.Zero) Raylib.UnloadMusicStream(_workstationSong);
            if (_level4Song.Stream.Buffer != IntPtr.Zero) Raylib.UnloadMusicStream(_level4Song);
            if (_level4WorkstationSong.Stream.Buffer != IntPtr.Zero) Raylib.UnloadMusicStream(_level4WorkstationSong);
            Console.WriteLine("AudioManager: All music unloaded.");
        }

        public Music GetSongByName(string name)
        {
            switch (name)
            {
                case "game_song": return _gameSong;
                case "workstation_song": return _workstationSong;
                case "level4_song": return _level4Song;
                case "level4_workstation": return _level4WorkstationSong;
                default: return new Music();
            }
        }
    }
}