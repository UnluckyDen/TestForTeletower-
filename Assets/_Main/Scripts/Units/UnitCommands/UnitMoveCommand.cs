using System.Collections;
using UnityEngine;

namespace _Main.Scripts.Units.UnitCommands
{
    public class UnitMoveUnitCommand : IUnitCommand
    {
        private readonly Vector3 _targetPoint;
        private readonly BaseUnit _baseUnit;
        
        public BaseUnit ActedUnit => _baseUnit;

        public UnitMoveUnitCommand(Vector3 targetPoint, BaseUnit baseUnit)
        {
            _targetPoint = targetPoint;
            _baseUnit = baseUnit;
        }

        public IEnumerator Execute()
        {
            _baseUnit.SetTargetPoint(_targetPoint);
            yield return null;
          
            while (_baseUnit.IsMoving) 
                yield return null;
            
            yield break;
        }
    }
}