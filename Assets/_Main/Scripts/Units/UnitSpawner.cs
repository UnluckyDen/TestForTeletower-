using _Main.Scripts.Settings;
using Unity.Netcode;
using UnityEngine;

namespace _Main.Scripts.Units
{
    public class UnitSpawner : NetworkBehaviour
    {
        [SerializeField] private BaseUnit _baseUnit;
        
        public BaseUnit SpawnUnit(PlayerSide playerSide, Vector3 position, Vector3 rotation)
        {
            BaseUnit baseUnit = Instantiate(_baseUnit, position, Quaternion.Euler(rotation));
            baseUnit.NetworkObject.Spawn();
            baseUnit.UpdatePlayerSideClientRpc(playerSide);
            baseUnit.UpdateMaterialClientRpc(playerSide);
            UnitRegistry.Instance.AddUnit(baseUnit);

            return baseUnit;
        }
    }
}