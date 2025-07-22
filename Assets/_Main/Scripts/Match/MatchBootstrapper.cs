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

        private readonly List<BaseUnit> _allUnits = new();

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
            
            _mapGenerator.GenerateMapClientRpc(NetworkManager.Singleton.ServerTime.Tick);

            SpawnUnitsForPlayer(player1Id, _spawnPointsPlayer1, PlayerSide.Side1);
            SpawnUnitsForPlayer(player2Id, _spawnPointsPlayer2, PlayerSide.Side2);
            
            _navMeshBaker.Bake();

            _matchController.InitializeMatch(_allUnits, player1Id, player2Id);
        }

        private void SpawnUnitsForPlayer(ulong ownerClientId, Transform[] spawnPoints, PlayerSide side)
        {
            foreach (var point in spawnPoints)
            {
                BaseUnit unit = _unitSpawner.SpawnUnit(side, point.position, point.eulerAngles);

                _allUnits.Add(unit);
            }
        }
    }
}
