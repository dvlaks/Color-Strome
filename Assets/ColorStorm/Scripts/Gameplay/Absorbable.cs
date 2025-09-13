using UnityEngine;

namespace ColorStorm.Gameplay
{
    /// <summary>
    /// Absorbable component acts as a tag to identify objects that can be absorbed by the player.
    /// Attach this component to any GameObject that should be consumable by the player's storm.
    /// The growthValue determines how much the player's storm will grow when this object is absorbed.
    /// </summary>
    public class Absorbable : MonoBehaviour
    {
        [Header("Absorption Settings")]
        [Tooltip("How much the player grows when absorbing this object")]
        public float growthValue = 0.1f;
    }
}