using System;
using System.Collections;
using _Main.Scripts.Match.Rounds;
using _Main.Scripts.Settings;
using _Main.Scripts.Units;
using _Main.Scripts.Units.Navigation;
using _Main.Scripts.Units.UnitCommands;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Main.Scripts.Match
{
    public class MatchController : NetworkBehaviour
    {
        public event Action MatchStarted;
        public event Action MatchEnded;

        [SerializeField] private UnitManipulator _unitManipulator;
        
        private MatchUnitsModel _matchUnitsModel;
        private RoundController _roundController;

        public static MatchController Instance { get; private set; }
        public MatchModel MatchModel { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Update() =>
            _roundController?.UpdateStates();

        [ClientRpc]
        public void InitializeMatchClientRpc(ulong player1Id, ulong player2Id, ulong[] unitsSide1, ulong[] unitsSide2, int seed)
        {
            _matchUnitsModel = new MatchUnitsModel(unitsSide1, unitsSide2);
            
            Random.InitState(seed);

            int startSide = Random.Range(1, 3);

            MatchModel = new MatchModel(player1Id, player2Id, 0, 15, (PlayerSide) startSide, 1);

            StartMatch();
        }

        private void StartMatch()
        {
            _roundController = new RoundController(MatchModel, _unitManipulator);

            if (IsServer)
            {
                _roundController.TurnEnded += StartNewTurnClientRpc;
            }
            
            AllUnitsToIdleState();

            _roundController.StartRound();
            MatchStarted?.Invoke();
        }

        private void EndMatch()
        {
            if (IsServer)
            { 
                _roundController.TurnEnded -= StartNewTurnClientRpc;
            }
        }

        public bool CanUseUnit(ulong playerId, ulong unitId)
        {
            PlayerSide turnSide = MatchModel.CurrentSide;
            PlayerSide playerSide = MatchModel.GetPlayerSide(playerId);
            PlayerSide unitSide = _matchUnitsModel.GetUnitSide(unitId);
            
            return (playerSide == unitSide) 
                   && (playerSide == turnSide);
        }

        [ClientRpc]
        private void StartNewTurnClientRpc()
        {
            MatchModel.AddActedPlayers(MatchModel.CurrentSide);
            PlayerSide newSide = MatchModel.CurrentSide == PlayerSide.Side1 ? PlayerSide.Side2 : PlayerSide.Side1;
            MatchModel.UpdateCurrentSide(newSide);

            if (MatchModel.ContainsActedSide(PlayerSide.Side1) && MatchModel.ContainsActedSide(PlayerSide.Side2))
            {
                MatchModel.UpdateRoundNumber(MatchModel.RoundNumber + 1);
                MatchModel.ClearActedPlayers();
            }
            
            AllUnitsToIdleState();

            _roundController.StartRound();
        }

        [ServerRpc(RequireOwnership = false)]
        public void SendUnitCommandServerRpc(UnitCommandData commandData)
        {
            IUnitCommand unitCommand = CommandFactory.GetUnitCommand(commandData); 
            WaitCommandExecute(unitCommand);
        }

        private async UniTask WaitCommandExecute(IUnitCommand unitCommand)
        {
            await unitCommand.Execute();
            StartNewTurnClientRpc();
        }

        private void AllUnitsToIdleState()
        {
            foreach (var unitId in _matchUnitsModel.GetAllUnitsBySide(PlayerSide.Side1))
            {
                UnitRegistry.Instance.TryGetUnit(unitId,out var unit);
                unit.ToIdleState();
            }
            
            foreach (var unitId in _matchUnitsModel.GetAllUnitsBySide(PlayerSide.Side2))
            {
                UnitRegistry.Instance.TryGetUnit(unitId,out var unit);
                unit.ToIdleState();
            }
        }
    }
}