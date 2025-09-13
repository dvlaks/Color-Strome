using UnityEngine;

namespace ColorStorm.Audio
{
    /// <summary>
    /// SoundLibrary is a ScriptableObject that stores audio clips for the Color Storm game.
    /// This centralized audio asset allows for easy management and organization of sound effects.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSoundLibrary", menuName = "Color Storm/Sound Library")]
    public class SoundLibrary : ScriptableObject
    {
        [Header("Game Sound Effects")]
        [Tooltip("Sound played when a player absorbs a neutral object")]
        public AudioClip absorbObjectSound;
    }
}