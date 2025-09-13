using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

namespace ColorStorm.Systems
{
    /// <summary>
    /// PlayFabManager is a persistent singleton that handles PlayFab integration for Color Storm.
    /// It manages player authentication, data storage, and backend communication with PlayFab services.
    /// </summary>
    public class PlayFabManager : MonoBehaviour
    {
        /// <summary>
        /// Static singleton instance for global access
        /// </summary>
        private static PlayFabManager _instance;
        
        /// <summary>
        /// Public accessor for the singleton instance
        /// </summary>
        public static PlayFabManager Instance => _instance;
        
        /// <summary>
        /// Gets whether the player is currently logged in to PlayFab
        /// </summary>
        public bool IsLoggedIn { get; private set; } = false;
        
        /// <summary>
        /// The player's PlayFab ID once logged in
        /// </summary>
        public string PlayFabId { get; private set; } = string.Empty;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Implement singleton pattern with persistence across scenes
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                
                Debug.Log("PlayFabManager initialized and set to persist across scenes");
            }
            else
            {
                // Destroy duplicate instances
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            // Begin the PlayFab login process
            Login();
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
        
        #region Authentication
        
        /// <summary>
        /// Performs anonymous login to PlayFab using the device's unique identifier
        /// </summary>
        private void Login()
        {
            Debug.Log("PlayFabManager: Starting anonymous login...");
            
            // Create login request using device unique identifier for anonymous authentication
            var request = new LoginWithCustomIDRequest
            {
                CustomId = SystemInfo.deviceUniqueIdentifier,
                CreateAccount = true
            };
            
            // Perform the login request
            PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
        }
        
        /// <summary>
        /// Called when PlayFab login succeeds
        /// </summary>
        /// <param name="result">The login result containing player information</param>
        private void OnLoginSuccess(LoginResult result)
        {
            IsLoggedIn = true;
            PlayFabId = result.PlayFabId;
            
            Debug.Log($"PlayFabManager: Login successful! Player ID: {result.PlayFabId}");
            
            // TODO: Add additional initialization logic here
            // - Load player data
            // - Initialize leaderboards
            // - Set up analytics tracking
        }
        
        /// <summary>
        /// Called when PlayFab login fails
        /// </summary>
        /// <param name="error">The error details from PlayFab</param>
        private void OnLoginFailure(PlayFabError error)
        {
            IsLoggedIn = false;
            PlayFabId = string.Empty;
            
            Debug.LogError($"PlayFabManager: Login failed! {error.GenerateErrorReport()}");
            
            // TODO: Add error handling logic here
            // - Retry mechanism
            // - Offline mode fallback
            // - User notification
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Attempts to retry the login process
        /// </summary>
        public void RetryLogin()
        {
            if (!IsLoggedIn)
            {
                Debug.Log("PlayFabManager: Retrying login...");
                Login();
            }
            else
            {
                Debug.Log("PlayFabManager: Already logged in, no need to retry");
            }
        }
        
        /// <summary>
        /// Logs out the current player (clears local state)
        /// </summary>
        public void Logout()
        {
            IsLoggedIn = false;
            PlayFabId = string.Empty;
            
            Debug.Log("PlayFabManager: Player logged out");
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Gets the device unique identifier used for anonymous login
        /// </summary>
        /// <returns>The device unique identifier</returns>
        public string GetDeviceId()
        {
            return SystemInfo.deviceUniqueIdentifier;
        }
        
        /// <summary>
        /// Checks if PlayFab services are available
        /// </summary>
        /// <returns>True if services are available, false otherwise</returns>
        public bool IsPlayFabAvailable()
        {
            // Simple check - could be enhanced with actual connectivity testing
            return Application.internetReachability != NetworkReachability.NotReachable;
        }
        
        #endregion
    }
}