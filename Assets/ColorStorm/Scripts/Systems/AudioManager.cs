using UnityEngine;
using ColorStorm.Audio;

namespace ColorStorm.Systems
{
    /// <summary>
    /// AudioManager is a persistent singleton that handles audio playback throughout the Color Storm game.
    /// It manages sound effects using a centralized SoundLibrary and provides easy-to-use static methods
    /// for playing sounds from anywhere in the codebase.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Configuration")]
        [SerializeField] private SoundLibrary soundLibrary;
        
        /// <summary>
        /// The AudioSource component used to play sound effects
        /// </summary>
        private AudioSource _audioSource;
        
        /// <summary>
        /// Static singleton instance for global access
        /// </summary>
        private static AudioManager _instance;
        
        /// <summary>
        /// Public accessor for the singleton instance
        /// </summary>
        public static AudioManager Instance => _instance;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Implement singleton pattern with persistence across scenes
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                
                // Initialize AudioSource component
                InitializeAudioSource();
                
                Debug.Log("AudioManager initialized and set to persist across scenes");
            }
            else
            {
                // Destroy duplicate instances
                Destroy(gameObject);
            }
        }
        
        private void OnDestroy()
        {
            // Clear the singleton instance when destroyed
            if (_instance == this)
            {
                _instance = null;
            }
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Ensures an AudioSource component exists on this GameObject
        /// </summary>
        private void InitializeAudioSource()
        {
            // Check if an AudioSource component exists
            _audioSource = GetComponent<AudioSource>();
            
            // If not, add one
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                Debug.Log("AudioManager: Added AudioSource component");
            }
            
            // Configure the AudioSource for sound effects
            _audioSource.playOnAwake = false;
            _audioSource.loop = false;
        }
        
        #endregion
        
        #region Public Static Methods
        
        /// <summary>
        /// Plays a sound effect using the AudioManager's AudioSource
        /// </summary>
        /// <param name="clip">The AudioClip to play</param>
        public static void PlaySound(AudioClip clip)
        {
            // Check if the manager instance exists and the clip is valid
            if (_instance != null && _instance._audioSource != null && clip != null)
            {
                _instance._audioSource.PlayOneShot(clip);
            }
            else
            {
                if (_instance == null)
                {
                    Debug.LogWarning("AudioManager: Cannot play sound - AudioManager instance not found");
                }
                else if (clip == null)
                {
                    Debug.LogWarning("AudioManager: Cannot play sound - AudioClip is null");
                }
            }
        }
        
        /// <summary>
        /// Plays the absorption sound effect from the SoundLibrary
        /// </summary>
        public static void PlayAbsorbSound()
        {
            if (_instance != null && _instance.soundLibrary != null)
            {
                PlaySound(_instance.soundLibrary.absorbObjectSound);
            }
            else
            {
                if (_instance == null)
                {
                    Debug.LogWarning("AudioManager: Cannot play absorb sound - AudioManager instance not found");
                }
                else if (_instance.soundLibrary == null)
                {
                    Debug.LogWarning("AudioManager: Cannot play absorb sound - SoundLibrary not assigned");
                }
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Sets the master volume for sound effects
        /// </summary>
        /// <param name="volume">Volume level between 0.0 and 1.0</param>
        public void SetMasterVolume(float volume)
        {
            if (_audioSource != null)
            {
                _audioSource.volume = Mathf.Clamp01(volume);
            }
        }
        
        /// <summary>
        /// Gets the current master volume
        /// </summary>
        /// <returns>Current volume level between 0.0 and 1.0</returns>
        public float GetMasterVolume()
        {
            return _audioSource != null ? _audioSource.volume : 0f;
        }
        
        /// <summary>
        /// Mutes or unmutes all sound effects
        /// </summary>
        /// <param name="muted">True to mute, false to unmute</param>
        public void SetMuted(bool muted)
        {
            if (_audioSource != null)
            {
                _audioSource.mute = muted;
            }
        }
        
        /// <summary>
        /// Gets whether sound effects are currently muted
        /// </summary>
        /// <returns>True if muted, false if not</returns>
        public bool IsMuted()
        {
            return _audioSource != null && _audioSource.mute;
        }
        
        #endregion
        
        #region Validation
        
        #if UNITY_EDITOR
        /// <summary>
        /// Editor validation - called when values change in the inspector
        /// </summary>
        private void OnValidate()
        {
            if (soundLibrary == null)
            {
                Debug.LogWarning("AudioManager: SoundLibrary is not assigned!");
            }
        }
        #endif
        
        #endregion
    }
}