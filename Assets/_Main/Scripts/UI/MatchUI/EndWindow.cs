using _Main.Scripts.Match;
using _Main.Scripts.Settings;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace _Main.Scripts.UI.MatchUI
{
    public class EndWindow : MonoBehaviour
    {
        [SerializeField] private TMP_Text _winSideText;
        [SerializeField] private SideSettings _settings;
        
        private void Start()
        {
            gameObject.SetActive(false);
            MatchController.Instance.MatchEnded += Show;
        }

        private void OnDestroy()
        {
            MatchController.Instance.MatchEnded -= Show;
        }

        public void Show(PlayerSide playerSide)
        {
            gameObject.SetActive(true);
            _winSideText.text =
                $"Win <color #{_settings.SideCollors[playerSide].ToHexString()}>{_settings.SideNames[playerSide]}</color>";
        }
    }
}