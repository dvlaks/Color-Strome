using UnityEngine;
using Fusion;
using ColorStorm.Systems;
using ColorStorm.UI;

namespace ColorStorm.Gameplay
{
    /// <summary>
    /// PlayerController handles the core movement mechanics for the player's storm using Photon Fusion networking.
    /// Uses Fusion's networked input system with drag-to-move controls where the player's
    /// storm smoothly follows the target position received from network input.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(StormIdentity))]
    public class PlayerController : NetworkBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5.0f;
        [SerializeField] private float movementSmoothness = 0.1f;
        [SerializeField] private float growthFactor = 1.0f;
        [SerializeField] private float absorptionFactor = 0.5f;
        
        // Networked Properties - Automatically synchronized across the network
        [Networked] private Vector2 NetworkedTargetPosition { get; set; }
        
        // Private fields
        private Camera mainCamera;
        private Rigidbody2D playerRigidbody;
        private StormIdentity stormIdentity;
        private Vector2 currentVelocity;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Cache component references for performance
            playerRigidbody = GetComponent<Rigidbody2D>();
            stormIdentity = GetComponent<StormIdentity>();
            mainCamera = Camera.main;
            
            // Validate that we have a main camera
            if (mainCamera == null)
            {
                Debug.LogError($"PlayerController on {gameObject.name}: No main camera found! Please tag your camera as 'MainCamera'.");
            }
        }
        
        public override void Spawned()
        {
            // Initialize the networked target position to current position when spawned
            if (Object.HasInputAuthority)
            {
                NetworkedTargetPosition = transform.position;
                
                // Register this local player with the GameHUD for size tracking
                GameHUD.Instance?.SetLocalPlayer(stormIdentity);
            }
        }
        
        #endregion
        
        #region Fusion Network Update
        
        /// <summary>
        /// Fusion's main update loop for networked objects. Runs at fixed timestep.
        /// Handles input processing and movement for objects with input authority.
        /// </summary>
        public override void FixedUpdateNetwork()
        {
            // Only process input if this client has input authority over this object
            if (Object.HasInputAuthority)
            {
                // Get the latest network input data from the client
                if (GetInput(out NetworkInputData inputData))
                {
                    // Update the networked target position with input from the client
                    NetworkedTargetPosition = inputData.targetPosition;
                }
            }
            
            // Apply movement towards the networked target position (runs on all clients)
            MoveTowardsTarget();
        }
        
        #endregion
        
        #region Movement
        
        /// <summary>
        /// Smoothly moves the player towards the networked target position using frame-rate independent interpolation
        /// </summary>
        private void MoveTowardsTarget()
        {
            // Calculate the current position as Vector2
            Vector2 currentPosition = transform.position;
            
            // Use SmoothDamp for natural, eased movement towards target
            // This provides smooth acceleration and deceleration
            // Note: Using Runner.DeltaTime for Fusion's fixed timestep
            Vector2 newPosition = Vector2.SmoothDamp(
                currentPosition, 
                NetworkedTargetPosition, 
                ref currentVelocity, 
                movementSmoothness, 
                moveSpeed, 
                Runner.DeltaTime
            );
            
            // Apply the new position using Rigidbody2D for proper physics integration
            playerRigidbody.MovePosition(newPosition);
        }
        
        #endregion
        
        #region Growth Mechanics
        
        /// <summary>
        /// Handles collision detection for absorbing objects and other players.
        /// Called when another 2D collider enters this object's trigger collider.
        /// Physics collisions are handled by the server in Fusion.
        /// </summary>
        /// <param name="other">The collider that entered this trigger</param>
        private void OnTriggerEnter2D(Collider2D other)
        {
            // Only process collisions on the server (object with state authority)
            if (!Object.HasStateAuthority) return;
            
            // First, check if the collided object has an Absorbable component (neutral objects)
            Absorbable absorbableComponent = other.GetComponent<Absorbable>();
            
            if (absorbableComponent != null)
            {
                // Handle absorption of neutral objects (original logic)
                float baseGrowthValue = absorbableComponent.growthValue;
                float finalGrowthAmount = baseGrowthValue * growthFactor;
                
                stormIdentity.AddSize(finalGrowthAmount);
                
                // Play absorption sound effect
                AudioManager.PlayAbsorbSound();
                
                // Notify the ObjectSpawner that an object was destroyed so it can decrement its counter
                ObjectSpawner.OnObjectDestroyed();
                Destroy(other.gameObject);
                
                Debug.Log($"Player absorbed neutral object {other.gameObject.name} and grew by {finalGrowthAmount}. New size: {stormIdentity.CurrentSize}");
            }
            // If no Absorbable component, check for player-vs-player interaction
            else if (other.GetComponent<StormIdentity>() != null)
            {
                StormIdentity otherStorm = other.GetComponent<StormIdentity>();
                
                // Rule A: Different Colors - Can only absorb storms of different colors
                // Rule B: Larger Size - Can only absorb storms that are smaller than us
                if (otherStorm.Color != stormIdentity.Color && stormIdentity.CurrentSize > otherStorm.CurrentSize)
                {
                    // Calculate growth amount based on the absorbed storm's size and our absorption factor
                    float growthAmount = otherStorm.CurrentSize * absorptionFactor;
                    
                    // Absorb the other storm and grow
                    stormIdentity.AddSize(growthAmount);
                    
                    // Destroy the absorbed storm
                    Destroy(other.gameObject);
                    
                    Debug.Log($"Player absorbed enemy storm ({otherStorm.Color}) with size {otherStorm.CurrentSize} and grew by {growthAmount}. New size: {stormIdentity.CurrentSize}");
                }
                // Team Merge: Handle same-color storm interactions (friendly merge)
                else if (otherStorm.Color == stormIdentity.Color)
                {
                    // Merge Rule: Only the larger storm initiates the merge to prevent both from merging simultaneously
                    // Tie-Breaker Rule: If sizes are equal, use GameObject instance ID as deterministic tie-breaker
                    bool shouldInitiateMerge = stormIdentity.CurrentSize > otherStorm.CurrentSize ||
                                             (stormIdentity.CurrentSize == otherStorm.CurrentSize && 
                                              gameObject.GetInstanceID() > other.gameObject.GetInstanceID());
                    
                    if (shouldInitiateMerge)
                    {
                        // Merge with allied storm - gain the full size (no absorption factor penalty)
                        float mergeAmount = otherStorm.CurrentSize;
                        stormIdentity.AddSize(mergeAmount);
                        
                        // Destroy the merged storm
                        Destroy(other.gameObject);
                        
                        Debug.Log($"Player merged with allied storm ({otherStorm.Color}) with size {otherStorm.CurrentSize} and grew by {mergeAmount}. New size: {stormIdentity.CurrentSize}");
                    }
                    else
                    {
                        // This storm is smaller or lost the tie-breaker, so let the other storm handle the merge
                        Debug.Log($"Waiting for larger allied storm to initiate merge (Our size: {stormIdentity.CurrentSize}, Their size: {otherStorm.CurrentSize})");
                    }
                }
                // If absorption rules are not met, log why (helpful for debugging)
                else
                {
                    if (stormIdentity.CurrentSize <= otherStorm.CurrentSize)
                    {
                        Debug.Log($"Cannot absorb larger enemy storm (Our size: {stormIdentity.CurrentSize}, Their size: {otherStorm.CurrentSize})");
                    }
                }
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Sets the movement speed of the player
        /// </summary>
        /// <param name="speed">New movement speed</param>
        public void SetMoveSpeed(float speed)
        {
            moveSpeed = Mathf.Max(0f, speed); // Ensure speed is not negative
        }
        
        /// <summary>
        /// Gets the current movement speed
        /// </summary>
        /// <returns>Current movement speed</returns>
        public float GetMoveSpeed()
        {
            return moveSpeed;
        }
        
        /// <summary>
        /// Gets the current networked target position the player is moving towards
        /// </summary>
        /// <returns>Networked target position</returns>
        public Vector2 GetTargetPosition()
        {
            return NetworkedTargetPosition;
        }
        
        /// <summary>
        /// Returns true if this client has input authority over (controls) this player.
        /// Renamed from HasInputAuthority() to IsControlledByLocalPlayer() to avoid compiler warning
        /// and improve semantic clarity for external callers.
        /// </summary>
        /// <returns>True if the local client controls this player instance.</returns>
        public bool IsControlledByLocalPlayer()
        {
            return Object.HasInputAuthority;
        }
        
        #endregion
        
        #region Debug
        
        private void OnDrawGizmosSelected()
        {
            // Draw a line from current position to target position for debugging
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, NetworkedTargetPosition);
            
            // Draw a small sphere at the target position
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(NetworkedTargetPosition, 0.2f);
        }
        
        #endregion
    }
}