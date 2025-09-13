using UnityEngine;
using Fusion;

namespace ColorStorm.Systems
{
    /// <summary>
    /// GameManager is a networked singleton that manages the overall state of the Color Storm match.
    /// It handles the match timer, game state transitions, and provides a central access point
    /// for other systems to query game information.
    /// </summary>
    public class GameManager : NetworkBehaviour
    {
        [Header("Match Configuration")]
        [SerializeField] private float matchDuration = 120f;
        
        /// <summary>
        /// The networked match timer using Fusion's TickTimer
        /// </summary>
        [Networked] private TickTimer MatchTimer { get; set; }
        
        /// <summary>
        /// Static singleton instance for easy access from other scripts
        /// </summary>
        public static GameManager Instance { get; private set; }
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Implement singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
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
        
        #region Fusion Lifecycle
        
        /// <summary>
        /// Called when this NetworkObject is spawned
        /// </summary>
        public override void Spawned()
        {
            // Only the server/host initializes the match timer
            if (Object.HasStateAuthority)
            {
                // Initialize the timer with the configured match duration
                MatchTimer = TickTimer.CreateFromSeconds(Runner, matchDuration);
                Debug.Log($"Match started! Duration: {matchDuration} seconds");
            }
        }
        
        /// <summary>
        /// Fusion's fixed network update - handles timer logic
        /// </summary>
        public override void FixedUpdateNetwork()
        {
            // Check if the timer is running and has expired
            if (MatchTimer.IsRunning && MatchTimer.Expired(Runner))
            {
                Debug.Log("Match Over!");
                
                // Stop the timer to prevent the message from logging every frame
                MatchTimer = TickTimer.None;
                
                // TODO: Add match end logic here (show results, transition to lobby, etc.)
            }
        }
        
        #endregion
        
        #region Public Properties
        
        /// <summary>
        /// Gets the remaining time in the match (in seconds)
        /// Can be used by UI systems to display countdown timers
        /// </summary>
        public float TimeRemaining => MatchTimer.RemainingTime(Runner) ?? 0f;
        
        /// <summary>
        /// Gets whether the match is currently active
        /// </summary>
        public bool IsMatchActive => MatchTimer.IsRunning;
        
        /// <summary>
        /// Gets the total match duration configured for this game
        /// </summary>
        public float MatchDuration => matchDuration;
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Starts a new match (only works with state authority)
        /// </summary>
        public void StartMatch()
        {
            if (!Object.HasStateAuthority) return;
            
            MatchTimer = TickTimer.CreateFromSeconds(Runner, matchDuration);
            Debug.Log($"Match restarted! Duration: {matchDuration} seconds");
        }
        
        /// <summary>
        /// Ends the current match immediately (only works with state authority)
        /// </summary>
        public void EndMatch()
        {
            if (!Object.HasStateAuthority) return;
            
            MatchTimer = TickTimer.None;
            Debug.Log("Match ended by GameManager");
        }
        
        /// <summary>
        /// Gets the elapsed time since the match started
        /// </summary>
        /// <returns>Elapsed time in seconds</returns>
        public float GetElapsedTime()
        {
            if (!MatchTimer.IsRunning) return 0f;
            return matchDuration - TimeRemaining;
        }
        
        /// <summary>
        /// Gets the match progress as a normalized value (0.0 to 1.0)
        /// </summary>
        /// <returns>Progress from 0.0 (start) to 1.0 (end)</returns>
        public float GetMatchProgress()
        {
            if (matchDuration <= 0f) return 1f;
            return Mathf.Clamp01(GetElapsedTime() / matchDuration);
        }
        
        #endregion
    }
}