
using System.Threading;
using _Main.Scripts.Infrastructure.StateMachine;
using Cysharp.Threading.Tasks;
using UnityEngine.AI;

namespace _Main.Scripts.Units.UnitsStateMachine
{
    public class IdleState : IState
    {
        private NavMeshAgent _navMeshAgent;
        private NavMeshObstacle _navMeshObstacle;

        private CancellationTokenSource _cancellationTokenSource;

        public IdleState(NavMeshAgent navMeshAgent, NavMeshObstacle navMeshObstacle)
        {
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
            _navMeshAgent.enabled = false;
            await UniTask.Yield();
            
            if (_cancellationTokenSource.IsCancellationRequested)
                return;
            
            _navMeshObstacle.enabled = true;
        }
    }
}