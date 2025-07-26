using System;
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
        public event Action<PlayerSide> MatchEnded;

        [SerializeField] private UnitManipulator _unitManipulator;
        [SerializeField] private int _roundTime;
        [SerializeField] private int _maxRoundCount = 15;

        private MatchUnitsModel _matchUnitsModel;
        private RoundController _roundController;

        public static MatchController Instance { get; private set; }
        public MatchModel MatchModel { get; private set; }

        private bool _waitingCommandExecuting = false;
        private bool _startingNewTurn = false;

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
        public void InitializeMatchClientRpc(ulong player1Id, ulong player2Id, ulong[] unitsSide1, ulong[] unitsSide2,
            int seed)
        {
            _matchUnitsModel = new MatchUnitsModel(unitsSide1, unitsSide2);

            Random.InitState(seed);

            int startSide = Random.Range(1, 3);

            MatchModel = new MatchModel(player1Id, player2Id, 0, _roundTime, (PlayerSide)startSide, 1, _maxRoundCount);
            MatchModel.SetAttackAvailable(true);
            MatchModel.SetMoveAvailable(true);

            StartMatch();
        }

        private void StartMatch()
        {
            _roundController = new RoundController(MatchModel, _unitManipulator);

            if (IsServer)
            {
                _roundController.TurnEnded += RoundControllerOnTurnEnded;
            }

            AllUnitsToIdleState();

            _roundController.StartRound();
            MatchStarted?.Invoke();
        }

        [ClientRpc]
        private void EndMatchClientRpc(PlayerSide winSide)
        {
            MatchEnded?.Invoke(winSide);
            if (IsServer)
            {
                _roundController.TurnEnded -= RoundControllerOnTurnEnded;
            }
        }

        public bool CanUseUnit(ulong playerId, ulong unitId)
        {
            PlayerSide turnSide = MatchModel.CurrentSide;
            PlayerSide playerSide = MatchModel.GetPlayerSide(playerId);
            PlayerSide unitSide = _matchUnitsModel.GetUnitSide(unitId);

            return playerSide == unitSide
                   && playerSide == turnSide;
        }

        public bool CanAttackUnit(ulong playerId, ulong unitId, ulong unitToAttackId)
        {
            PlayerSide turnSide = MatchModel.CurrentSide;
            PlayerSide playerSide = MatchModel.GetPlayerSide(playerId);
            PlayerSide unitSide = _matchUnitsModel.GetUnitSide(unitId);
            PlayerSide unitToAttackSide = _matchUnitsModel.GetUnitSide(unitToAttackId);

            UnitRegistry.Instance.TryGetUnit(unitId, out var unit);
            UnitRegistry.Instance.TryGetUnit(unitToAttackId, out var unitToAttack);

            return turnSide == unitSide
                   && playerSide == turnSide
                   && unitSide != unitToAttackSide
                   && unit.CanAttackReachUnit(unitToAttack);
        }

        [ClientRpc]
        private void StartNewTurnClientRpc()
        {
            MatchModel.AddActedPlayers(MatchModel.CurrentSide);
            PlayerSide newSide = MatchModel.CurrentSide == PlayerSide.Side1 ? PlayerSide.Side2 : PlayerSide.Side1;
            MatchModel.UpdateCurrentSide(newSide);
            MatchModel.SetAttackAvailable(true);
            MatchModel.SetMoveAvailable(true);

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
            if (_waitingCommandExecuting)
                return;

            IUnitCommand unitCommand = CommandFactory.GetUnitCommand(commandData);
            WaitCommandExecute(unitCommand);
        }

        [ServerRpc(RequireOwnership = false)]
        public void AttackUnitServerRpc(ulong unitToAttackId)
        {
            UnitRegistry.Instance.TryGetUnit(unitToAttackId, out BaseUnit unitToAttack);
            AttackUnitClientRpc(unitToAttack.PlayerSide, unitToAttackId);
            unitToAttack.NetworkObject.Despawn();
            Destroy(unitToAttack.gameObject);
        }

        [ClientRpc]
        private void AttackUnitClientRpc(PlayerSide playerSide, ulong unitToAttack)
        {
            _matchUnitsModel.RemoveUnit(playerSide, unitToAttack);
        }

        private async UniTask WaitCommandExecute(IUnitCommand unitCommand)
        {
            _waitingCommandExecuting = true;

            switch (unitCommand)
            {
                case UnitMoveUnitCommand when !MatchModel.MoveAvailable:
                    _waitingCommandExecuting = false;
                    return;
                case UnitMoveUnitCommand:
                    UpdateMoveAvailableClientRpc(false);
                    break;
                case UnitAttackUnitCommand when !MatchModel.AttackAvailable:
                    _waitingCommandExecuting = false;
                    return;
                case UnitAttackUnitCommand:
                    UpdateAttackAvailableClientRpc(false);
                    break;
            }

            await unitCommand.Execute();

            if (!CheckSideAvailableActions())
                StartNewTurnAfterCommandExecuting();
            
            _waitingCommandExecuting = false;
        }

        [ClientRpc]
        private void UpdateMoveAvailableClientRpc(bool available) =>
            MatchModel.SetMoveAvailable(available);

        [ClientRpc]
        private void UpdateAttackAvailableClientRpc(bool available) =>
            MatchModel.SetAttackAvailable(available);

        private void RoundControllerOnTurnEnded() =>
            StartNewTurnAfterCommandExecuting();

        private async UniTask StartNewTurnAfterCommandExecuting()
        {
            if (_startingNewTurn)
                return;

            _startingNewTurn = true;
            while (_waitingCommandExecuting)
                await UniTask.Yield();

            if (CheckSideWin())
            {
                _startingNewTurn = false;
                return;
            }

            StartNewTurnClientRpc();
            _startingNewTurn = false;
        }

        private void AllUnitsToIdleState()
        {
            foreach (var unitId in _matchUnitsModel.GetAllUnitsBySide(PlayerSide.Side1))
            {
                UnitRegistry.Instance.TryGetUnit(unitId, out var unit);
                unit.ToIdleState();
            }

            foreach (var unitId in _matchUnitsModel.GetAllUnitsBySide(PlayerSide.Side2))
            {
                UnitRegistry.Instance.TryGetUnit(unitId, out var unit);
                unit.ToIdleState();
            }
        }

        private bool CheckSideAvailableActions()
        {
            if (!MatchModel.MoveAvailable && !MatchModel.AttackAvailable)
                return false;

            PlayerSide oppositeSide = MatchModel.CurrentSide == PlayerSide.Side1 ? PlayerSide.Side2 : PlayerSide.Side1;
            bool sideCanAttackAnyUnit = false;
            foreach (var unitId in _matchUnitsModel.GetAllUnitsBySide(MatchModel.CurrentSide))
            {
                if (!UnitRegistry.Instance.TryGetUnit(unitId, out BaseUnit unit))
                    continue;

                if (!unit.CanAttackReachAnyUnitSide(oppositeSide)) 
                    continue;
                
                sideCanAttackAnyUnit = true;
                break;
            }

            if (!MatchModel.MoveAvailable && MatchModel.AttackAvailable && !sideCanAttackAnyUnit)
                return false;
                
            return true;
        }

        private bool CheckSideWin()
        {
            int side1 = _matchUnitsModel.GetAllUnitsBySide(PlayerSide.Side1).Count;
            int side2 = _matchUnitsModel.GetAllUnitsBySide(PlayerSide.Side2).Count;

            if (side1 == 0 || (MatchModel.RoundNumber > MatchModel.MaxRoundNumber && side2 > side1))
            {
                EndMatchClientRpc(PlayerSide.Side2);
                return true;
            }

            if (side2 == 0 || (MatchModel.RoundNumber > MatchModel.MaxRoundNumber && side1 > side2))
            {
                EndMatchClientRpc(PlayerSide.Side1);
                return true;
            }

            if (MatchModel.RoundNumber >= MatchModel.MaxRoundNumber 
                && side1 == side2)
            {
                foreach (var baseUnit in UnitRegistry.Instance.GetAllUnits())
                    baseUnit.UpdateSpeedClientRpc(float.MaxValue);
            }

            return false;
        }
    }
}