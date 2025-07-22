using System.Collections.Generic;
using UnityEngine;

namespace _Main.Scripts.Units
{
    public class UnitRegistry : MonoBehaviour
    {
        private readonly Dictionary<ulong, BaseUnit> _units = new();
        public static UnitRegistry Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void AddUnit(BaseUnit unit)
        {
            _units[unit.NetworkObjectId] = unit;
        }

        public void RemoveUnit(BaseUnit unit)
        {
            _units.Remove(unit.NetworkObjectId);
        }

        public bool TryGetUnit(ulong id, out BaseUnit unit)
        {
            return _units.TryGetValue(id, out unit);
        }
    }
}