using UnityEngine;
using ColorStorm.Data;

namespace ColorStorm.Gameplay
{
    /// <summary>
    /// StormVisuals handles the visual appearance of storms by applying the correct color
    /// to the sprite based on the storm's identity and a ColorMapping asset.
    /// This component ensures that storm colors are visually consistent throughout the game.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(StormIdentity))]
    public class StormVisuals : MonoBehaviour
    {
        [Header("Visual Settings")]
        [SerializeField] private ColorMapping colorMapping;
        
        // Cached component references
        private SpriteRenderer spriteRenderer;
        private StormIdentity stormIdentity;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Cache component references for performance
            spriteRenderer = GetComponent<SpriteRenderer>();
            stormIdentity = GetComponent<StormIdentity>();
            
            // Validate that we have the required ColorMapping asset
            if (colorMapping == null)
            {
                Debug.LogError($"StormVisuals on {gameObject.name}: ColorMapping asset is not assigned! Please assign a ColorMapping ScriptableObject.");
            }
        }
        
        private void Start()
        {
            // Apply the initial color based on the storm's identity
            ApplyColor();
        }
        
        #endregion
        
        #region Color Management
        
        /// <summary>
        /// Applies the correct color to the sprite based on the storm's color identity
        /// </summary>
        private void ApplyColor()
        {
            // Ensure we have all required components and assets
            if (colorMapping == null || stormIdentity == null || spriteRenderer == null)
            {
                Debug.LogWarning($"StormVisuals on {gameObject.name}: Cannot apply color - missing required components or ColorMapping asset.");
                return;
            }
            
            // Get the storm's color from its identity
            StormColor stormColor = stormIdentity.Color;
            
            // Look up the corresponding Unity Color using the ColorMapping asset
            Color displayColor = colorMapping.GetColorFromEnum(stormColor);
            
            // Apply the color to the sprite renderer
            spriteRenderer.color = displayColor;
            
            // Optional: Log the color application for debugging
            Debug.Log($"StormVisuals: Applied color {displayColor} for StormColor.{stormColor} to {gameObject.name}");
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Updates the visual color of the storm. Call this method when the storm's color changes.
        /// </summary>
        public void UpdateColor()
        {
            ApplyColor();
        }
        
        /// <summary>
        /// Sets a new ColorMapping asset and immediately applies the color
        /// </summary>
        /// <param name="newColorMapping">The new ColorMapping asset to use</param>
        public void SetColorMapping(ColorMapping newColorMapping)
        {
            colorMapping = newColorMapping;
            ApplyColor();
        }
        
        /// <summary>
        /// Gets the currently assigned ColorMapping asset
        /// </summary>
        /// <returns>The current ColorMapping asset, or null if none is assigned</returns>
        public ColorMapping GetColorMapping()
        {
            return colorMapping;
        }
        
        #endregion
        
        #region Editor Helpers
        
        /// <summary>
        /// Forces a color update in the editor (useful for testing different color mappings)
        /// </summary>
        [ContextMenu("Force Color Update")]
        private void ForceColorUpdate()
        {
            // Re-cache components in case they've changed
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (stormIdentity == null) stormIdentity = GetComponent<StormIdentity>();
            
            ApplyColor();
        }
        
        #endregion
    }
}