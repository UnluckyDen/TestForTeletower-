using _Main.Scripts.Units;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace _Main.Scripts.Settings
{
    [CreateAssetMenu(fileName = "NewMatchUnitSettings", menuName = "ScriptableObjects/MatchUnitSettings", order = 1)]
    public class MatchUnitSettings : ScriptableObject
    {
        [field: SerializeField] public SerializedDictionary<UnitType, BaseUnit> UnitTypes;
    }
}