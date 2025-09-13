using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ColorStorm.Systems;

namespace ColorStorm.Systems
{
    /// <summary>
    /// NetworkManager handles the setup and management of Photon Fusion networking for Color Storm.
    /// This script serves as the main entry point for starting multiplayer sessions,
    /// spawning players, and gathering input from local clients to send to the server.
    /// </summary>
    public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        [Header("Network Configuration")]
        [SerializeField] private NetworkPrefabRef _playerPrefab;
        
        // Private network runner instance
        private NetworkRunner _runner;
        
        #region Network Initialization
        
        /// <summary>
        /// Initializes and starts a Fusion NetworkRunner with the specified game mode
        /// </summary>
        /// <param name="mode">The game mode (Host, Client, Server, etc.)</param>
        async void StartGame(GameMode mode)
        {
            // Create the Fusion runner and let it know that we will be providing user input
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;
            
            // Add this script as a callback handler to receive network events
            _runner.AddCallbacks(this);
            
            // Create the networking session configuration
            var sceneInfo = new NetworkSceneInfo();
            if (SceneManager.GetActiveScene().IsValid())
            {
                sceneInfo.AddSceneRef(SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex), LoadSceneMode.Single);
            }
            
            // Start the networking session
            var result = await _runner.StartGame(new StartGameArgs()
            {
                GameMode = mode,
                SessionName = "ColorStormSession",
                Scene = sceneInfo,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
            });
            
            // Check if the session started successfully
            if (result.Ok == false)
            {
                Debug.LogError($"Failed to start NetworkRunner: {result.ShutdownReason}");
            }
            else
            {
                Debug.Log($"NetworkRunner started successfully in {mode} mode");
            }
        }
        
        #endregion
        
        #region UI Interface Methods
        
        /// <summary>
        /// Called by UI Button to start a game as Host
        /// </summary>
        public void OnHostClicked()
        {
            Debug.Log("Starting game as Host...");
            StartGame(GameMode.Host);
        }
        
        /// <summary>
        /// Called by UI Button to join a game as Client
        /// </summary>
        public void OnClientClicked()
        {
            Debug.Log("Joining game as Client...");
            StartGame(GameMode.Client);
        }
        
        #endregion
        
        #region INetworkRunnerCallbacks Implementation
        
        /// <summary>
        /// Called when a player joins the session. Spawns a player object for them.
        /// </summary>
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"Player {player} joined the session");
            
            // Only the server spawns player objects
            if (runner.IsServer)
            {
                // Spawn the player prefab with input authority assigned to the joining player
                Vector3 spawnPosition = Vector3.zero; // You can randomize this later
                NetworkObject playerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
                
                Debug.Log($"Spawned player object for {player} at {spawnPosition}");
            }
        }
        
        /// <summary>
        /// Called every network tick to gather input from the local player.
        /// This is where we collect mouse/touch input and send it to the server.
        /// </summary>
        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            // Create our custom input data structure
            var networkInputData = new NetworkInputData();
            
            // Only gather input if we have a valid camera
            if (Camera.main != null)
            {
                // Read the current mouse position in screen coordinates
                Vector3 mousePosition = Input.mousePosition;
                
                // Convert screen coordinates to world coordinates for 2D gameplay
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0f));
                
                // Assign the world position to our input data
                networkInputData.targetPosition = worldPosition;
            }
            
            // Send the input data to Fusion for network transmission
            input.Set(networkInputData);
        }
        
        /// <summary>
        /// Called when a player leaves the session
        /// </summary>
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"Player {player} left the session");
        }
        
        /// <summary>
        /// Called when this client connects to the server
        /// </summary>
        public void OnConnectedToServer(NetworkRunner runner)
        {
            Debug.Log("Connected to server");
        }
        
        /// <summary>
        /// Called when this client disconnects from the server
        /// </summary>
        public void OnDisconnectedFromServer(NetworkRunner runner)
        {
            Debug.Log("Disconnected from server");
        }
        
        /// <summary>
        /// Called when there's a connection failure
        /// </summary>
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            Debug.LogError($"Connection failed to {remoteAddress}: {reason}");
        }
        
        /// <summary>
        /// Called when the session shuts down
        /// </summary>
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            Debug.Log($"NetworkRunner shutdown: {shutdownReason}");
        }
        
        /// <summary>
        /// Called when input becomes unavailable for a player
        /// </summary>
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
            // Handle missing input - could use last known input or default values
        }
        
        /// <summary>
        /// Called when the connect request to a session is sent
        /// </summary>
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
            // Handle connection requests - can implement authentication here
        }
        
        /// <summary>
        /// Called when session list is updated
        /// </summary>
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            // Handle session list updates for lobby/matchmaking
        }
        
        /// <summary>
        /// Called when custom properties are updated
        /// </summary>
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
            // Handle custom authentication responses
        }
        
        /// <summary>
        /// Called when host migration occurs
        /// </summary>
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            // Handle host migration for robustness
        }
        
        /// <summary>
        /// Called for reliable data messages
        /// </summary>
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
        {
            // Handle reliable data transmission
        }
        
        /// <summary>
        /// Called when scene loading progress updates
        /// </summary>
        public void OnSceneLoadDone(NetworkRunner runner)
        {
            Debug.Log("Scene loading completed");
        }
        
        /// <summary>
        /// Called when scene loading starts
        /// </summary>
        public void OnSceneLoadStart(NetworkRunner runner)
        {
            Debug.Log("Scene loading started");
        }

        // Added required INetworkRunnerCallbacks stubs (new SDK signatures)
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        
        #endregion
        
        #region Cleanup
        
        private void OnApplicationPause(bool pauseStatus)
        {
            // Handle application pause for mobile
            if (_runner != null && pauseStatus)
            {
                _runner.Shutdown();
            }
        }
        
        private void OnDestroy()
        {
            // Clean up the network runner when this object is destroyed
            if (_runner != null)
            {
                _runner.Shutdown();
            }
        }
        
        #endregion
    }
}