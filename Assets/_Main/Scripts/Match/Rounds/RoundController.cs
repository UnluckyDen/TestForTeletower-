using System;
using _Main.Scripts.Infrastructure.StateMachine;
using _Main.Scripts.Units.Navigation;

namespace _Main.Scripts.Match.Rounds
{
    public class RoundController : BaseStateMachine
    {
        public event Action TurnEnded;
        
        private MatchModel _matchModel;
        private UnitManipulator _unitManipulator;
        
        public RoundController(MatchModel matchModel, UnitManipulator unitManipulator)
        {
            _matchModel = matchModel;
            _unitManipulator = unitManipulator;
        }

        public void StartRound()
        {
            var turnState = new TurnState(this, _matchModel, _unitManipulator);
            ToState(turnState); 
        }

        public void EndTurn() =>
            TurnEnded?.Invoke();
    }
}