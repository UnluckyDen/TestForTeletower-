using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using _Main.Scripts.Map;
using _Main.Scripts.Settings;
using _Main.Scripts.Units;
using _Main.Scripts.Units.Navigation;
using AYellowpaper.SerializedCollections;

namespace _Main.Scripts.Match
{
    public class MatchBootstrapper : NetworkBehaviour
    {
        [SerializeField] private MapGenerator _mapGenerator;
        [SerializeField] private NavMeshBaker _navMeshBaker;
        [SerializeField] private UnitSpawner _unitSpawner;
        
        [SerializeField] private MatchController _matchController;
        
        [SerializeField] private SerializedDictionary<PlayerSide, List<UnitType>> _unitsForSides;
        
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

            List<ulong> unitsSide1 = SpawnUnitsForPlayer(PlayerSide.Side1);
            List<ulong> unitsSide2 = SpawnUnitsForPlayer(PlayerSide.Side2);
            
            _navMeshBaker.BakeClientRpc();

            _matchController.InitializeMatchClientRpc(player1Id, player2Id, unitsSide1.ToArray(), unitsSide2.ToArray(), seed);
        }

        private List<ulong> SpawnUnitsForPlayer(PlayerSide side)
        {
            List<ulong> spawnedUnits = new List<ulong>();

            List<BaseUnit> units = _unitSpawner.SpawnUnits(side, _unitsForSides[side]);
            
            foreach (var unit in units)
                spawnedUnits.Add(unit.NetworkObjectId);
            
            return spawnedUnits;
        }
    }
}
