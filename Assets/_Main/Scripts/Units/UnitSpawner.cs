using System.Collections.Generic;
using _Main.Scripts.Map;
using _Main.Scripts.Settings;
using AYellowpaper.SerializedCollections;
using Unity.Netcode;
using UnityEngine;

namespace _Main.Scripts.Units
{
    public class UnitSpawner : NetworkBehaviour
    {
        [SerializeField] private MatchUnitSettings _units;

        [SerializeField] private SerializedDictionary<PlayerSide, SpawnZone> _zones;

        public List<BaseUnit> SpawnUnits(PlayerSide playerSide, List<UnitType> unitTypes)
        {
            List<BaseUnit> units = new List<BaseUnit>();
            Vector3[] positions = _zones[playerSide].GetPositions(unitTypes.Count, 1.1f);
            int index = 0;
            foreach (var unitType in unitTypes)
            {
                units.Add(SpawnUnit(playerSide, unitType, positions[index]));
                index++;
            }

            return units;
        }

        private BaseUnit SpawnUnit(PlayerSide playerSide, UnitType unitType, Vector3 position)
        {
            BaseUnit baseUnit = Instantiate(_units.UnitTypes[unitType], position, Quaternion.identity);
            baseUnit.NetworkObject.Spawn();
            baseUnit.UpdatePlayerSideClientRpc(playerSide);
            baseUnit.UpdateMaterialClientRpc(playerSide);

            return baseUnit;
        }
    }
}