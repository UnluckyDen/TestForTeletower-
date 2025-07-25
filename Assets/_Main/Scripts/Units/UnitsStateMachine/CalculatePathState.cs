using System.Threading;
using _Main.Scripts.Infrastructure.StateMachine;
using Cysharp.Threading.Tasks;
using UnityEngine.AI;

namespace _Main.Scripts.Units.UnitsStateMachine
{
    public class CalculatePathState : IState
    {
        private UnitStateMachine _unitStateMachine;
        private NavMeshAgent _navMeshAgent;
        private NavMeshObstacle _navMeshObstacle;

        private CancellationTokenSource _cancellationTokenSource;

        public CalculatePathState(UnitStateMachine unitStateMachine,NavMeshAgent navMeshAgent, NavMeshObstacle navMeshObstacle)
        {
            _unitStateMachine = unitStateMachine;
            _navMeshAgent = navMeshAgent;
            _navMeshObstacle = navMeshObstacle;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Enter()
        {
            SwitchObastacleActive();
        }

        public void Update()
        {
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
        }
        
        private async UniTask SwitchObastacleActive()
        {
            _navMeshObstacle.enabled = false;
            await _unitStateMachine.WaitUntilNavMeshIsUpdated(_cancellationTokenSource.Token);

            if (_cancellationTokenSource.IsCancellationRequested)
                return;
            
            _navMeshAgent.enabled = true;
        }
    }
}