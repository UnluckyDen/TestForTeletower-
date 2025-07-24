using System.Collections;

namespace _Main.Scripts.Units.UnitCommands
{
    public interface IUnitCommand
    {
        public BaseUnit ActedUnit { get; }
        public IEnumerator Execute();
    }
}