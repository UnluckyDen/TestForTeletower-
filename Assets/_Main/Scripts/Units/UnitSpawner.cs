using System;
using _Main.Scripts.Settings;
using Unity.Netcode;
using UnityEngine;

namespace _Main.Scripts.Units
{
    public class UnitSpawner : NetworkBehaviour
    {
        [SerializeField] private BaseUnit _baseUnit;
        [SerializeField] private Transform _spawnPointTransform;

        private void SpawnUnit(PlayerSide playerSide)
        {
            SpawnUnitServerRpc(playerSide);
        }
        
        [ContextMenu("SpawnUnit_Side1")]
        private void SpawnUnitSide1()
        {
            SpawnUnitServerRpc(PlayerSide.Side1);
        }
        
        [ContextMenu("SpawnUnit_Side2")]
        private void SpawnUnitSide2()
        {
            SpawnUnitServerRpc(PlayerSide.Side2);
        }

        [ServerRpc]
        private void SpawnUnitServerRpc(PlayerSide playerSide)
        {
            BaseUnit baseUnit = Instantiate(_baseUnit, _spawnPointTransform.position, _spawnPointTransform.rotation);
            baseUnit.NetworkObject.Spawn();
            baseUnit.UpdateMaterialClientRpc(playerSide);
        }
    }
}