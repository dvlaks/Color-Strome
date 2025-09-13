using UnityEngine;
using Fusion;

namespace ColorStorm.Gameplay
{
    /// <summary>
    /// Defines the possible colors for storms in the game
    /// </summary>
    public enum StormColor
    {
        Neutral,
        Red,
        Blue,
        Green,
        Yellow
    }

    /// <summary>
    /// StormIdentity manages the core properties of any storm in the game.
    /// This component handles the storm's color and size, which are synchronized
    /// across the network by Photon Fusion.
    /// </summary>
    public class StormIdentity : NetworkBehaviour
    {
        private const float MIN_SIZE = 0.1f;

        [Networked] 
        public StormColor Color { get; set; } = StormColor.Neutral;

        [Networked] 
        public float CurrentSize { get; set; } = 1.0f;

        // Local variable to track the previous size for change detection
        private float _previousSize;

        public override void Spawned()
        {
            // Initialize the previous size when the object is spawned
            _previousSize = CurrentSize;
        }

        public override void Render()
        {
            // Update the visual scale every frame based on the current networked size
            transform.localScale = Vector3.one * CurrentSize;

            // Manual check to see if the size has changed since the last frame
            if (CurrentSize != _previousSize)
            {
                HandleSizeChanged();
                _previousSize = CurrentSize; // Update the previous size
            }
        }

        /// <summary>
        /// This method is now called manually from Render() when a size change is detected.
        /// </summary>
        private void HandleSizeChanged()
        {
            // We can add visual or sound effects here in the future.
            // This will be called on all clients when the value changes.
            // Debug.Log($"Size changed to: {CurrentSize}");
        }
        
        /// <summary>
        /// Increases the storm's size. Only works if this object has state authority.
        /// </summary>
        public void AddSize(float amount)
        {
            if (!Object.HasStateAuthority) return;
            
            CurrentSize += amount;
            CurrentSize = Mathf.Max(CurrentSize, MIN_SIZE);
        }
        
        /// <summary>
        /// Sets the storm's color. Only works if this object has state authority.
        /// </summary>
        public void SetColor(StormColor newColor)
        {
            if (!Object.HasStateAuthority) return;
            
            Color = newColor;
        }
    }
}