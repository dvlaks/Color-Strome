using UnityEngine;
using TMPro;
using ColorStorm.Systems;
using ColorStorm.Gameplay;

namespace ColorStorm.UI
{
    /// <summary>
    /// GameHUD manages the player's Heads-Up Display, showing essential game information
    /// like the match timer and the local player's current size.
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        [Header("UI Text References")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI sizeText;
        
        /// <summary>
        /// Reference to the local player's StormIdentity for displaying size information
        /// </summary>
        private StormIdentity _localPlayerIdentity;
        
        /// <summary>
        /// Static singleton instance for easy access from other scripts
        /// </summary>
        public static GameHUD Instance { get; private set; }
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Implement singleton pattern
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            // Initialize UI elements with default values
            if (timerText != null)
            {
                timerText.text = "--:--";
            }
            
            if (sizeText != null)
            {
                sizeText.text = "Size: --";
            }
        }
        
        private void Update()
        {
            UpdateTimerDisplay();
            UpdateSizeDisplay();
        }
        
        private void OnDestroy()
        {
            // Clear the singleton instance when destroyed
            if (Instance == this)
            {
                Instance = null;
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Sets the reference to the local player's StormIdentity for size tracking
        /// </summary>
        /// <param name="localPlayer">The local player's StormIdentity component</param>
        public void SetLocalPlayer(StormIdentity localPlayer)
        {
            _localPlayerIdentity = localPlayer;
            Debug.Log($"GameHUD: Local player set to {(_localPlayerIdentity != null ? _localPlayerIdentity.gameObject.name : "null")}");
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Updates the timer display with the current match time remaining
        /// </summary>
        private void UpdateTimerDisplay()
        {
            // Check if GameManager exists and timerText is assigned
            if (GameManager.Instance != null && timerText != null)
            {
                // Get the remaining time from the GameManager
                float timeRemaining = GameManager.Instance.TimeRemaining;
                
                // Format the time into MM:SS format
                string formattedTime = FormatTime(timeRemaining);
                
                // Update the timer text
                timerText.text = formattedTime;
            }
        }
        
        /// <summary>
        /// Updates the size display with the local player's current size
        /// </summary>
        private void UpdateSizeDisplay()
        {
            // Check if we have a local player reference and sizeText is assigned
            if (_localPlayerIdentity != null && sizeText != null)
            {
                // Get the current size from the local player's StormIdentity
                float currentSize = _localPlayerIdentity.CurrentSize;
                
                // Format the size to one decimal place and update the text
                sizeText.text = $"Size: {currentSize:F1}";
            }
        }
        
        /// <summary>
        /// Formats time in seconds to MM:SS string format
        /// </summary>
        /// <param name="timeInSeconds">Time in seconds to format</param>
        /// <returns>Formatted time string in MM:SS format</returns>
        private string FormatTime(float timeInSeconds)
        {
            // Ensure we don't display negative time
            timeInSeconds = Mathf.Max(0f, timeInSeconds);
            
            // Calculate minutes and seconds
            int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
            
            // Return formatted string with leading zeros
            return $"{minutes:D2}:{seconds:D2}";
        }
        
        #endregion
        
        #region Validation Methods
        
        /// <summary>
        /// Validates that all required UI components are assigned
        /// </summary>
        private void ValidateComponents()
        {
            if (timerText == null)
            {
                Debug.LogWarning("GameHUD: Timer Text component is not assigned!");
            }
            
            if (sizeText == null)
            {
                Debug.LogWarning("GameHUD: Size Text component is not assigned!");
            }
        }
        
        #if UNITY_EDITOR
        /// <summary>
        /// Editor validation - called when values change in the inspector
        /// </summary>
        private void OnValidate()
        {
            ValidateComponents();
        }
        #endif
        
        #endregion
    }
}