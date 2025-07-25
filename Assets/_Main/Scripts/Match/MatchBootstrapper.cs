using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using _Main.Scripts.Map;
using _Main.Scripts.Settings;
using _Main.Scripts.Units;
using _Main.Scripts.Units.Navigation;

namespace _Main.Scripts.Match
{
    public class MatchBootstrapper : NetworkBehaviour
    {
        [SerializeField] private MapGenerator _mapGenerator;
        [SerializeField] private NavMeshBaker _navMeshBaker;
        [SerializeField] private UnitSpawner _unitSpawner;
        
        [SerializeField] private MatchController _matchController;
        
        [SerializeField] private Transform[] _spawnPointsPlayer1;
        [SerializeField] private Transform[] _spawnPointsPlayer2;
        
        private readonly List<ulong> _connectedClients = new();

        public override void OnNetworkSpawn() =>
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        public override void OnNetworkDespawn() =>
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;

        private void OnClientConnected(ulong clientId)
        {
            if (!IsServer)
                return;

            _connectedClients.Add(clientId);
            
            if (_connectedClients.Count == 2)
                StartMatch();
        }

        private void StartMatch()
        {
            ulong player1Id = _connectedClients[0];
            ulong player2Id = _connectedClients[1];

            int seed = NetworkManager.Singleton.ServerTime.Tick;
            _mapGenerator.GenerateMapClientRpc(seed);

            List<ulong> unitsSide1 = SpawnUnitsForPlayer(_spawnPointsPlayer1, PlayerSide.Side1);
            List<ulong> unitsSide2 = SpawnUnitsForPlayer(_spawnPointsPlayer2, PlayerSide.Side2);
            
            _navMeshBaker.BakeClientRpc();

            _matchController.InitializeMatchClientRpc(player1Id, player2Id, unitsSide1.ToArray(), unitsSide2.ToArray(), seed);
        }

        private List<ulong> SpawnUnitsForPlayer(Transform[] spawnPoints, PlayerSide side)
        {
            List<ulong> spawnedUnits = new List<ulong>();
            foreach (var point in spawnPoints)
            {
                BaseUnit unit = _unitSpawner.SpawnUnit(side, point.position, point.eulerAngles);

                spawnedUnits.Add(unit.NetworkObjectId);
            }
            
            return spawnedUnits;
        }
    }
}
