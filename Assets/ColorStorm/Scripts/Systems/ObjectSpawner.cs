using UnityEngine;
using Fusion;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ColorStorm.Systems
{
    /// <summary>
    /// ObjectSpawner is responsible for spawning neutral, absorbable objects in the arena.
    /// It uses server-authoritative spawning to ensure consistent object distribution
    /// across all clients and maintains a count of active objects.
    /// </summary>
    public class ObjectSpawner : NetworkBehaviour
    {
        [Header("Spawn Configuration")]
        [SerializeField] private NetworkPrefabRef neutralObjectPrefab;
        [SerializeField] private int maxObjectsToSpawn = 50;
        [SerializeField] private float spawnRadius = 25f;
        [SerializeField] private float spawnInterval = 0.2f;
        
        /// <summary>
        /// Timer to control the spawn rate
        /// </summary>
        private TickTimer _spawnTimer;
        
        /// <summary>
        /// Networked counter of currently active objects in the arena
        /// </summary>
        [Networked] private int CurrentObjectCount { get; set; }
        
        /// <summary>
        /// Static reference to the spawner instance for easy access by other scripts
        /// </summary>
        private static ObjectSpawner _instance;
        
        #region Fusion Lifecycle
        
        /// <summary>
        /// Called when this NetworkObject is spawned
        /// </summary>
        public override void Spawned()
        {
            // Set the static instance reference
            _instance = this;
            
            // Initialize the spawn timer if we have state authority
            if (Object.HasStateAuthority)
            {
                _spawnTimer = TickTimer.CreateFromSeconds(Runner, spawnInterval);
                Debug.Log($"ObjectSpawner initialized. Max objects: {maxObjectsToSpawn}, Spawn radius: {spawnRadius}");
            }
        }
        
        /// <summary>
        /// Fusion's fixed network update - handles server-authoritative spawning logic
        /// </summary>
        public override void FixedUpdateNetwork()
        {
            // Only the server/host can spawn objects
            if (!Object.HasStateAuthority) return;
            
            // Check if the spawn timer has expired or is not yet running
            if (_spawnTimer.ExpiredOrNotRunning(Runner))
            {
                // Reset the timer for the next interval
                _spawnTimer = TickTimer.CreateFromSeconds(Runner, spawnInterval);
                
                // Check if we are below the maximum number of objects
                if (CurrentObjectCount < maxObjectsToSpawn)
                {
                    // Calculate a random 2D position within the spawn radius
                    Vector2 randomDirection = Random.insideUnitCircle.normalized;
                    float randomDistance = Random.Range(0f, spawnRadius);
                    Vector3 spawnPosition = new Vector3(
                        randomDirection.x * randomDistance,
                        randomDirection.y * randomDistance,
                        0f
                    );
                    
                    // Spawn the neutral object and increment the counter
                    Runner.Spawn(
                        neutralObjectPrefab, 
                        spawnPosition, 
                        Quaternion.identity,
                        onBeforeSpawned: (runner, obj) => 
                        {
                            // Increment our counter
                            CurrentObjectCount++;
                        }
                    );
                    
                    Debug.Log($"Spawned neutral object at {spawnPosition}. Total objects: {CurrentObjectCount}/{maxObjectsToSpawn}");
                }
            }
        }
        
        private void OnDestroy()
        {
            // Clear the static instance when destroyed
            if (_instance == this)
            {
                _instance = null;
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Called by other scripts when an absorbable object is destroyed.
        /// Decrements the object counter to allow new objects to be spawned.
        /// </summary>
        public static void OnObjectDestroyed()
        {
            // Find the spawner instance and decrement the counter
            if (_instance != null && _instance.Object.HasStateAuthority)
            {
                _instance.CurrentObjectCount = Mathf.Max(0, _instance.CurrentObjectCount - 1);
                Debug.Log($"Object destroyed. Remaining objects: {_instance.CurrentObjectCount}/{_instance.maxObjectsToSpawn}");
            }
        }
        
        /// <summary>
        /// Gets the current number of active objects in the arena
        /// </summary>
        /// <returns>Current object count</returns>
        public int GetCurrentObjectCount()
        {
            return CurrentObjectCount;
        }
        
        /// <summary>
        /// Gets the maximum number of objects that can be spawned
        /// </summary>
        /// <returns>Maximum object count</returns>
        public int GetMaxObjectCount()
        {
            return maxObjectsToSpawn;
        }
        
        /// <summary>
        /// Gets the spawn progress as a normalized value (0.0 to 1.0)
        /// </summary>
        /// <returns>Spawn progress from 0.0 (empty) to 1.0 (full)</returns>
        public float GetSpawnProgress()
        {
            if (maxObjectsToSpawn <= 0) return 1f;
            return (float)CurrentObjectCount / maxObjectsToSpawn;
        }
        
        /// <summary>
        /// Forces a manual spawn if conditions allow (server authority only)
        /// </summary>
        public void ForceSpawn()
        {
            if (!Object.HasStateAuthority) return;
            if (CurrentObjectCount >= maxObjectsToSpawn) return;
            
            // Calculate a random 2D position within the spawn radius
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            float randomDistance = Random.Range(0f, spawnRadius);
            Vector3 spawnPosition = new Vector3(
                randomDirection.x * randomDistance,
                randomDirection.y * randomDistance,
                0f
            );
            
            // Spawn the neutral object and increment the counter
            Runner.Spawn(
                neutralObjectPrefab, 
                spawnPosition, 
                Quaternion.identity,
                onBeforeSpawned: (runner, obj) => 
                {
                    CurrentObjectCount++;
                }
            );
            
            Debug.Log($"Force spawned neutral object at {spawnPosition}. Total objects: {CurrentObjectCount}/{maxObjectsToSpawn}");
        }
        
        #endregion
        
        #region Debug Methods
        
        /// <summary>
        /// Draws the spawn radius in the Scene view for visual debugging
        /// </summary>
        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Use Handles for a proper 2D disc visualization in editor
            Handles.color = Color.cyan;
            Handles.DrawWireDisc(transform.position, Vector3.forward, spawnRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
        #endif
        
        #endregion
    }
}