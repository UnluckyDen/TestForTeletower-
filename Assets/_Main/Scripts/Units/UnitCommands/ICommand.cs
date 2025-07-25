using Cysharp.Threading.Tasks;

namespace _Main.Scripts.Units.UnitCommands
{
    public interface IUnitCommand
    {
        public BaseUnit ActedUnit { get; }
        public UniTask Execute();
    }
}