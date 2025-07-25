using System;
using System.Collections.Generic;
using _Main.Scripts.Settings;

namespace _Main.Scripts.Match
{
    public class MatchModel
    {
        public event Action ModelUpdated;

        public readonly ulong Player1Id;
        public readonly ulong Player2Id;

        private readonly Dictionary<ulong, PlayerSide> _playerSides;
        private HashSet<PlayerSide> _actedSides = new();

        public float CurrentTime { get; private set; }
        public float MaxTurnTime { get; private set; }
        public PlayerSide CurrentSide { get; private set; }
        public int RoundNumber { get; private set; }
        public bool WaitingToExecuteCommand { get; private set; }
        public bool AttackAvailable { get; private set; }
        public bool MoveAvailable { get; private set; }


        public MatchModel(ulong player1Id, ulong player2Id, float currentTime, float maxTurnTime,
            PlayerSide currentSide, int roundNumber)
        {
            Player1Id = player1Id;
            Player2Id = player2Id;

            _playerSides = new Dictionary<ulong, PlayerSide>
            {
                { Player1Id, PlayerSide.Side1 },
                { Player2Id, PlayerSide.Side2 }
            };
            
            CurrentTime = currentTime;
            MaxTurnTime = maxTurnTime;
            CurrentSide = currentSide;
            RoundNumber = roundNumber;
        }

        public PlayerSide GetPlayerSide(ulong playerId) =>
            _playerSides.GetValueOrDefault(playerId);
        
        public void UpdateCurrentTime(float currentTime)
        {
            CurrentTime = currentTime;
            ModelUpdated?.Invoke();
        }

        public void UpdateRoundNumber(int roundNumber)
        {
            RoundNumber = roundNumber;
            ModelUpdated?.Invoke();
        }

        public void UpdateCurrentSide(PlayerSide playerSide)
        {
            CurrentSide = playerSide;
            ModelUpdated?.Invoke();
        }

        public void UpdateWaitingExecuteCommand(bool waiting)
        {
            WaitingToExecuteCommand = waiting;
            ModelUpdated?.Invoke();
        }

        public void SetAttackAvailable(bool attackAvailable)
        {
            AttackAvailable = attackAvailable;
            ModelUpdated?.Invoke();
        }

        public void SetMoveAvailable(bool moveAvailable)
        {
            MoveAvailable = moveAvailable;
            ModelUpdated?.Invoke();
        }

        public void AddActedPlayers(PlayerSide actedSide) => 
            _actedSides.Add(actedSide);

        public bool ContainsActedSide(PlayerSide playerSide) =>
            _actedSides.Contains(playerSide);

        public void ClearActedPlayers() =>
            _actedSides.Clear();
    }
}