using System;
using Unity.AI.Navigation;
using Unity.Netcode;
using UnityEngine;

namespace _Main.Scripts.Units.Navigation
{
    public class NavMeshBaker : NetworkBehaviour
    {
        [SerializeField] private NavMeshSurface _navMeshSurface;
        
        [ClientRpc]
        public void BakeClientRpc()
        {
            _navMeshSurface.BuildNavMesh();
        }
    }
}