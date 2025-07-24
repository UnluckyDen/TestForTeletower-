using _Main.Scripts.Match;
using _Main.Scripts.Settings;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace _Main.Scripts.UI.MatchUI
{
    public class SideView : MonoBehaviour
    {
        [SerializeField] private SideSettings _unitSettings;
        [SerializeField] private TMP_Text _sideText;

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
            SetSide(_matchModel.GetPlayerSide(NetworkManager.Singleton.LocalClientId));
        }

        public void SetSide(PlayerSide playerSide)
        {
            _sideText.text = $"Your side is <color #{_unitSettings.SideCollors[playerSide].ToHexString()}>{_unitSettings.SideNames[playerSide]}</color>";
        }
    }
}