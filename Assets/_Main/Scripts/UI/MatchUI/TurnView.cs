using _Main.Scripts.Match;
using _Main.Scripts.Settings;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace _Main.Scripts.UI.MatchUI
{
    public class TurnView : MonoBehaviour
    {
        [SerializeField] private SideSettings _unitSettings;
        [SerializeField] private TMP_Text _roundNumber;
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private TMP_Text _sideTurn;
        [SerializeField] private TMP_Text _attackAvailable;
        [SerializeField] private TMP_Text _moveAvailable;

        private MatchModel _matchModel;

        private void Start()
        {
            gameObject.SetActive(false);
            MatchController.Instance.MatchStarted += MatchControllerOnMatchStarted;
            MatchController.Instance.MatchEnded += MatchControllerOnMatchEnded;
        }

        private void OnDestroy()
        {
            MatchController.Instance.MatchStarted -= MatchControllerOnMatchStarted;
            MatchController.Instance.MatchEnded -= MatchControllerOnMatchEnded;
        }

        private void MatchControllerOnMatchStarted()
        {
            gameObject.SetActive(true);
            _matchModel = MatchController.Instance.MatchModel;
            _matchModel.ModelUpdated += MatchModelOnModelUpdated;
            MatchModelOnModelUpdated();
        }

        private void MatchControllerOnMatchEnded()
        {
            gameObject.SetActive(false);
            _matchModel.ModelUpdated -= MatchModelOnModelUpdated;
            _matchModel = null;
        }

        private void MatchModelOnModelUpdated()
        {
            _roundNumber.text = $"Round number: {_matchModel.RoundNumber}";
            _timerText.text = _matchModel.WaitingToExecuteCommand ? "Waiting end of turn" : $"Round time {(int) _matchModel.CurrentTime}";
            _sideTurn.text = $"Now turn of <color #{_unitSettings.SideCollors[_matchModel.CurrentSide].ToHexString()}>{_unitSettings.SideNames[_matchModel.CurrentSide]}</color>";
            _moveAvailable.text = _matchModel.MoveAvailable ? $"Move available" : $"Move not available";
            _attackAvailable.text = _matchModel.AttackAvailable ? $"Attack available" : $"Attack not available";
        }
    }
}