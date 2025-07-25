using _Main.Scripts.Match;
using IState = _Main.Scripts.Infrastructure.StateMachine.IState;

namespace _Main.Scripts.Units.UnitsStateMachine
{
    public class AttackState : IState
    {
        private UnitStateMachine _unitStateMachine;
        private BaseUnit _unitToAttack;

        public AttackState(UnitStateMachine unitStateMachine, BaseUnit unitToAttack)
        {
            _unitStateMachine = unitStateMachine;
            _unitToAttack = unitToAttack;
        }

        public void Enter()
        {
            AttackUnit();
        }

        public void Update()
        {
        }

        public void Dispose()
        {
        }

        private void AttackUnit()
        {
            MatchController.Instance.AttackUnitServerRpc(_unitToAttack.NetworkObjectId);
            _unitStateMachine.ToIdleState();
        }
    }
}