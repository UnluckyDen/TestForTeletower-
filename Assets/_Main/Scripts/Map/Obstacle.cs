using UnityEngine;

namespace _Main.Scripts.Map
{
    public class Obstacle : MonoBehaviour
    {
        [field: SerializeField] public CorneredZone CorneredZone { get; private set; }
    }
}
