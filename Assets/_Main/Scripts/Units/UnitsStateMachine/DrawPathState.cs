using System.Threading;
using _Main.Scripts.Infrastructure.StateMachine;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace _Main.Scripts.Units.UnitsStateMachine
{
    public class DrawPathState : IState
    {
        private NavMeshAgent _navMeshAgent;
        private NavMeshObstacle _navMeshObstacle;
        private UnitStateMachine _unitStateMachine;
        private UnitPathPredictor _unitPathPredictor;

        private Vector3 _target;
        private CancellationTokenSource _cancellationTokenSource;

        public DrawPathState(UnitStateMachine unitStateMachine, NavMeshAgent navMeshAgent, NavMeshObstacle navMeshObstacle, UnitPathPredictor pathPredictor, Vector3 target)
        {
            _unitStateMachine = unitStateMachine;
            _navMeshAgent = navMeshAgent;
            _navMeshObstacle = navMeshObstacle;
            _unitPathPredictor = pathPredictor;
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
            _unitPathPredictor.ClearPath();
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
            
            DrawPath();
        }
        
        private void DrawPath()
        {
            Vector3[] path = _unitStateMachine.BaseUnit.CalculatePath(_target);
            if (path != null)
                _unitPathPredictor.DrawPath(path, _unitStateMachine.BaseUnit.Speed);
        }
    }
}