using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace _Main.Scripts.Settings
{
    [CreateAssetMenu(fileName = "NewSideSettings", menuName = "ScriptableObjects/SideSettings", order = 1)]
    public class SideSettings : ScriptableObject
    {
        [field: SerializeField] public SerializedDictionary<PlayerSide, Material> SideMaterials;
        [field: SerializeField] public SerializedDictionary<PlayerSide, Color> SideCollors;
        [field: SerializeField] public SerializedDictionary<PlayerSide, string> SideNames;
    }
}