using Cysharp.Threading.Tasks;
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

        public async UniTask Execute()
        {
            _baseUnit.ToMoveState(_targetPoint);
            await UniTask.Yield();
          
            while (_baseUnit.IsMoving) 
                await UniTask.Yield();
        }
    }
}