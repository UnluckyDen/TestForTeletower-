using System;

namespace _Main.Scripts.Units.UnitCommands
{
    public static class CommandFactory
    {
        public static IUnitCommand GetUnitCommand(UnitCommandData commandData)
        {
            UnitRegistry.Instance.TryGetUnit(commandData.UnitId, out BaseUnit unit);
            switch (commandData.UnitCommandType)
            {
                case UnitCommandType.MoveCommand:
                    return new UnitMoveUnitCommand(commandData.TargetPosition, unit);

                case UnitCommandType.AttackCommand:
                    UnitRegistry.Instance.TryGetUnit(commandData.TargetUnit, out BaseUnit targetUnit);
                    return new UnitAttackUnitCommand(unit, targetUnit);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}