using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace _Main.Scripts.Settings
{
    [CreateAssetMenu(fileName = "NewUnitSettings", menuName = "ScriptableObjects/UnitSettings", order = 1)]
    public class UnitSettings : ScriptableObject
    {
        [field: SerializeField] public SerializedDictionary<PlayerSide, Material> SideMaterials;
    }
}