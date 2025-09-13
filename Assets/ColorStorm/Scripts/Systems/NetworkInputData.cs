using UnityEngine;
using Fusion;

namespace ColorStorm.Systems
{
    /// <summary>
    /// NetworkInputData is a struct that implements INetworkInput for Photon Fusion.
    /// It carries input data from clients to the server, specifically the player's
    /// desired target position based on their mouse/finger input.
    /// 
    /// This struct is serialized and transmitted over the network, so it should
    /// remain lightweight and contain only essential input data.
    /// </summary>
    public struct NetworkInputData : INetworkInput
    {
        /// <summary>
        /// The target world position where the player wants to move based on their input.
        /// This is calculated from screen coordinates on the client and sent to the server.
        /// </summary>
        public Vector3 targetPosition;
    }
}