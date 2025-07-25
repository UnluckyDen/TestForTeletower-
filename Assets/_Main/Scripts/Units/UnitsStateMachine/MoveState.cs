using System.Threading;
using _Main.Scripts.Infrastructure.StateMachine;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace _Main.Scripts.Units.UnitsStateMachine
{
    public class MoveState : IState
    {
        private NavMeshAgent _navMeshAgent;
        private NavMeshObstacle _navMeshObstacle;
        private UnitStateMachine _unitStateMachine;

        private Vector3 _target;
        private CancellationTokenSource _cancellationTokenSource;

        public MoveState(UnitStateMachine unitStateMachine, NavMeshAgent navMeshAgent, NavMeshObstacle navMeshObstacle, Vector3 target)
        {
            _unitStateMachine = unitStateMachine;
            _navMeshAgent = navMeshAgent;
            _navMeshObstacle = navMeshObstacle;
            _target = target;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Enter()
        {
            SwitchObstacleActive();
        }

        public void Update()
        {
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
        }
        
        private async UniTask SwitchObstacleActive()
        {
            _navMeshObstacle.enabled = false;
            
            await _unitStateMachine.WaitUntilNavMeshIsUpdated(_cancellationTokenSource.Token,0.1f, 1f);
            
            if (_cancellationTokenSource.IsCancellationRequested)
                return;
            
            _navMeshAgent.enabled = true;
            await UniTask.Yield();
            
            if (_cancellationTokenSource.IsCancellationRequested)
                return;
            
            Move();
        }

        private async UniTask Move()
        {
            _navMeshAgent.SetDestination(_target);
            
            await UniTask.Yield();
            
            if (_cancellationTokenSource.IsCancellationRequested)
                return;

            bool isMoving = _navMeshAgent.hasPath
                            && _navMeshAgent.remainingDistance > _navMeshAgent.stoppingDistance;

            while (isMoving)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                    return;
                
                isMoving = _navMeshAgent.hasPath
                           && _navMeshAgent.remainingDistance > _navMeshAgent.stoppingDistance;

                await UniTask.Yield();
            }

            _unitStateMachine.ToIdleState();
        }
    }
}