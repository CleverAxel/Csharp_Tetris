using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace DirtyLibrary {
    public class AudioController : IDisposable {

        // Tracks sound effect instances created so they can be paused, unpaused, and/or disposed.
        private readonly List<SoundEffectInstance> _activeSoundEffectInstances;

        // Tracks the volume for song playback when muting and unmuting.
        private float _previousSongVolume;

        // Tracks the volume for sound effect playback when muting and unmuting.
        private float _previousSoundEffectVolume;

        /// <summary>
        /// Gets a value that indicates if audio is muted.
        /// </summary>
        public bool IsMuted { get; private set; }


        public Dictionary<string, SoundEffect> soundEffects = new Dictionary<string, SoundEffect>();
        public Dictionary<string, Song> songs = new Dictionary<string, Song>();

        public void LoadContent() {
            // var test = Core.Content.Load<SoundEffect>("audio/player_movement");
            soundEffects.Add("player_movement", Core.Content.Load<SoundEffect>("audio/player_movement"));
            soundEffects.Add("player_rotation", Core.Content.Load<SoundEffect>("audio/rotation"));
            soundEffects.Add("tetromino_spawn", Core.Content.Load<SoundEffect>("audio/pop"));
            soundEffects.Add("tetromino_disappear", Core.Content.Load<SoundEffect>("audio/disappear"));

            songs.Add("gravity", Core.Content.Load<Song>("audio/idonthavetherights"));
        }

        public float SongVolume {
            get {
                if (IsMuted) {
                    return 0.0f;
                }

                return MediaPlayer.Volume;
            }
            set {
                if (IsMuted) {
                    return;
                }

                MediaPlayer.Volume = Math.Clamp(value, 0.0f, 1.0f);
            }
        }

        public float SoundEffectVolume {
            get {
                if (IsMuted) {
                    return 0.0f;
                }

                return SoundEffect.MasterVolume;
            }
            set {
                if (IsMuted) {
                    return;
                }

                SoundEffect.MasterVolume = Math.Clamp(value, 0.0f, 1.0f);
            }
        }

        public bool IsDisposed { get; private set; }

        public AudioController() {
            _activeSoundEffectInstances = new List<SoundEffectInstance>();
        }

        public void Update() {
            for (int i = _activeSoundEffectInstances.Count - 1; i >= 0; i--) {
                SoundEffectInstance instance = _activeSoundEffectInstances[i];

                if (instance.State == SoundState.Stopped) {
                    if (!instance.IsDisposed) {
                        instance.Dispose();
                    }
                    _activeSoundEffectInstances.RemoveAt(i);
                }
            }
        }

        public SoundEffectInstance PlaySoundEffect(SoundEffect soundEffect) {
            return PlaySoundEffect(soundEffect, 1.0f, 1.0f, 0.0f, false);
        }


        public SoundEffectInstance PlaySoundEffect(SoundEffect soundEffect, float volume, float pitch, float pan, bool isLooped) {
            SoundEffectInstance soundEffectInstance = soundEffect.CreateInstance();

            soundEffectInstance.Volume = volume;
            soundEffectInstance.Pitch = pitch;
            soundEffectInstance.Pan = pan;
            soundEffectInstance.IsLooped = isLooped;

            soundEffectInstance.Play();
            _activeSoundEffectInstances.Add(soundEffectInstance);

            return soundEffectInstance;
        }

        public void PlaySong(Song song, bool isRepeating = true) {
            if (MediaPlayer.State == MediaState.Playing) {
                MediaPlayer.Stop();
            }
            MediaPlayer.Volume = 0.15f;
            MediaPlayer.Play(song);
            
            MediaPlayer.IsRepeating = isRepeating;
        }

        public void PauseAudio() {
            MediaPlayer.Pause();

            foreach (SoundEffectInstance soundEffectInstance in _activeSoundEffectInstances) {
                soundEffectInstance.Pause();
            }
        }

        public void ResumeAudio() {
            MediaPlayer.Resume();

            foreach (SoundEffectInstance soundEffectInstance in _activeSoundEffectInstances) {
                soundEffectInstance.Resume();
            }
        }

        public void MuteAudio() {
            _previousSongVolume = MediaPlayer.Volume;
            _previousSoundEffectVolume = SoundEffect.MasterVolume;

            MediaPlayer.Volume = 0.0f;
            SoundEffect.MasterVolume = 0.0f;

            IsMuted = true;
        }

        public void UnmuteAudio() {
            // Restore the previous volume values.
            MediaPlayer.Volume = _previousSongVolume;
            SoundEffect.MasterVolume = _previousSoundEffectVolume;

            IsMuted = false;
        }

        public void ToggleMute() {
            if (IsMuted) {
                UnmuteAudio();
            } else {
                MuteAudio();
            }
        }

        ~AudioController() => Dispose(false);

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing) {
            if (IsDisposed) {
                return;
            }

            if (disposing) {
                foreach (SoundEffectInstance soundEffectInstance in _activeSoundEffectInstances) {
                    soundEffectInstance.Dispose();
                }
                _activeSoundEffectInstances.Clear();
            }

            IsDisposed = true;
        }
    }
}

