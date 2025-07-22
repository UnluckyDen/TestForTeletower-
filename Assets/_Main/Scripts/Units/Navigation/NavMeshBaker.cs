using System;
using Unity.AI.Navigation;
using UnityEngine;

namespace _Main.Scripts.Units.Navigation
{
    public class NavMeshBaker : MonoBehaviour
    {
        [SerializeField] private NavMeshSurface _navMeshSurface;

        public void Start()
        {
            Bake();
        }

        [ContextMenu("Bake")]
        public void Bake()
        {
            _navMeshSurface.BuildNavMesh();
        }
    }
}