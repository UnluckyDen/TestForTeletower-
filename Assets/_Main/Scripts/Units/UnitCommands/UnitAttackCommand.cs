using Cysharp.Threading.Tasks;

namespace _Main.Scripts.Units.UnitCommands
{
    public class UnitAttackUnitCommand : IUnitCommand
    {
        private readonly BaseUnit _baseUnit;
        private readonly BaseUnit _targetUnit;
        
        public BaseUnit ActedUnit => _baseUnit;

        public UnitAttackUnitCommand(BaseUnit baseUnit, BaseUnit targetUnit)
        {
            _baseUnit = baseUnit;
            _targetUnit = targetUnit;
        }

        public async UniTask Execute()
        {
            _baseUnit.ToAttackState(_targetUnit);
        }
    }
}