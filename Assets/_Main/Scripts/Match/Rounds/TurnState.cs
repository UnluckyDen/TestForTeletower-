using _Main.Scripts.Infrastructure.StateMachine;
using _Main.Scripts.Units.Navigation;
using _Main.Scripts.Units.UnitCommands;
using UnityEngine;

namespace _Main.Scripts.Match.Rounds
{
    public class TurnState : IState
    {
        private readonly RoundController _roundController;
        private readonly MatchModel _matchModel;
        private readonly UnitManipulator _unitManipulator;
        
        private float _currentTime;

        public TurnState(RoundController roundController, MatchModel matchModel, UnitManipulator unitManipulator)
        {
            _roundController = roundController;
            _matchModel = matchModel;
            _unitManipulator = unitManipulator;
        }

        public void Enter()
        {
            _matchModel.UpdateCurrentTime(0);
            _matchModel.UpdateWaitingExecuteCommand(false);
            _unitManipulator.UnitCommandGiven += UnitManipulatorOnUnitCommandGiven;
        }

        public void Update()
        { 
            if (_matchModel.WaitingToExecuteCommand)
                return;
            
            UpdateTimer();
        }

        public void Dispose()
        {
            _unitManipulator.UnitCommandGiven -= UnitManipulatorOnUnitCommandGiven;
        }

        private void UnitManipulatorOnUnitCommandGiven(UnitCommandData command)
        {
            MatchController.Instance.SendUnitCommandServerRpc(command);
        }

        private void UpdateTimer()
        {
            _currentTime += Time.deltaTime;
            _matchModel.UpdateCurrentTime(_currentTime);

            if (_currentTime > _matchModel.MaxTurnTime)
            {
                _roundController.EndTurn(); 
            }
        }
    }
}