using System;
using System.Collections.Generic;
using UnityEngine;
using ColorStorm.Gameplay;

namespace ColorStorm.Data
{
    /// <summary>
    /// Represents a mapping between a StormColor enum value and its corresponding Unity Color
    /// </summary>
    [Serializable]
    public class ColorEntry
    {
        [Tooltip("The storm color enum value")]
        public StormColor stormColor;
        
        [Tooltip("The Unity color to display for this storm color")]
        public Color displayColor;
    }

    /// <summary>
    /// ColorMapping is a ScriptableObject that allows designers to map StormColor enum values
    /// to actual Unity Color values in the editor. This provides a centralized way to manage
    /// the visual appearance of different storm colors throughout the game.
    /// </summary>
    [CreateAssetMenu(fileName = "NewColorMapping", menuName = "Color Storm/Color Mapping")]
    public class ColorMapping : ScriptableObject
    {
        [Header("Color Mappings")]
        [Tooltip("List of mappings between StormColor enum values and their display colors")]
        public List<ColorEntry> colorEntries = new List<ColorEntry>();
        
        /// <summary>
        /// Gets the Unity Color that corresponds to the specified StormColor enum value
        /// </summary>
        /// <param name="stormColor">The StormColor enum value to look up</param>
        /// <returns>The corresponding Unity Color, or Color.magenta if no mapping is found</returns>
        public Color GetColorFromEnum(StormColor stormColor)
        {
            // Search through all color entries for a matching storm color
            foreach (ColorEntry entry in colorEntries)
            {
                if (entry.stormColor == stormColor)
                {
                    return entry.displayColor;
                }
            }
            
            // If no match is found, log a warning and return magenta as an error indicator
            Debug.LogWarning($"ColorMapping: No color mapping found for StormColor.{stormColor}. Returning Color.magenta as fallback.");
            return Color.magenta;
        }
        
        /// <summary>
        /// Checks if a mapping exists for the specified StormColor
        /// </summary>
        /// <param name="stormColor">The StormColor to check</param>
        /// <returns>True if a mapping exists, false otherwise</returns>
        public bool HasMapping(StormColor stormColor)
        {
            foreach (ColorEntry entry in colorEntries)
            {
                if (entry.stormColor == stormColor)
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Validates that all StormColor enum values have corresponding entries
        /// Call this method in the editor to check for missing mappings
        /// </summary>
        [ContextMenu("Validate All Mappings")]
        public void ValidateAllMappings()
        {
            StormColor[] allStormColors = (StormColor[])Enum.GetValues(typeof(StormColor));
            bool allMappingsValid = true;
            
            foreach (StormColor stormColor in allStormColors)
            {
                if (!HasMapping(stormColor))
                {
                    Debug.LogWarning($"ColorMapping: Missing mapping for StormColor.{stormColor}");
                    allMappingsValid = false;
                }
            }
            
            if (allMappingsValid)
            {
                Debug.Log("ColorMapping: All StormColor enum values have valid mappings!");
            }
        }
    }
}