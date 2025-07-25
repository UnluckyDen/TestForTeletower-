using System.Threading;
using _Main.Scripts.Infrastructure.StateMachine;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace _Main.Scripts.Units.UnitsStateMachine
{
    public class UnitStateMachine : BaseStateMachine
    {
        private BaseUnit _baseUnit;
        private NavMeshAgent _navMeshAgent;
        private NavMeshObstacle _navMeshObstacle;
        private UnitPathPredictor _unitPathPredicator;

        public BaseUnit BaseUnit => _baseUnit;

        public UnitStateMachine(BaseUnit baseUnit, NavMeshAgent navMeshAgent, NavMeshObstacle navMeshObstacle, UnitPathPredictor unitPathPredictor)
        {
            _baseUnit = baseUnit;
            _navMeshAgent = navMeshAgent;
            _navMeshObstacle = navMeshObstacle;
            _unitPathPredicator = unitPathPredictor;
        }

        public void ToIdleState() =>
            ToState(new IdleState(_navMeshAgent, _navMeshObstacle));

        public void ToMoveState(Vector3 target) =>
            ToState(new MoveState(this, _navMeshAgent, _navMeshObstacle, target));

        public void ToAttackState(BaseUnit unitToAttack) =>
            ToState(null);
        
        public void ToDrawPathState(Vector3 target)
        {ToState(new DrawPathState(this, _navMeshAgent, _navMeshObstacle, _unitPathPredicator, target));}

        public async UniTask WaitUntilNavMeshIsUpdated(CancellationToken cancellationToken, float checkRadius = 0.1f, float timeout = 1f)
        {
            float timer = 0f;

            while (timer < timeout)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                if (NavMesh.SamplePosition(BaseUnit.transform.position, out _, checkRadius, NavMesh.AllAreas))
                    return; 

                timer += Time.deltaTime;
                await UniTask.Yield(); 
            }
        }
    }
}