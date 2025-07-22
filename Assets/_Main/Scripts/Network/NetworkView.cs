using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace _Main.Scripts.Network
{
    public class NetworkView : MonoBehaviour
    {
        [SerializeField] private Button _hostButton;
        [SerializeField] private Button _clientButton;
        [SerializeField] private TMP_Text _statusText;

        private void OnEnable()
        {
            _hostButton.onClick.AddListener(StartHost);
            _clientButton.onClick.AddListener(StartClient);
        }

        private void OnDisable()
        {
            _hostButton.onClick.RemoveListener(StartHost);
            _clientButton.onClick.RemoveListener(StartClient);
        }

        private void StartHost()
        {
            NetworkManager.Singleton.StartHost();
            UpdateContent();
        }

        private void StartClient()
        {
            NetworkManager.Singleton.StartClient();
            UpdateContent();
        }

        private void UpdateContent()
        {
            UpdateStatus();
            UpdateButtons();
        }

        private void UpdateStatus()
        {
            _statusText.text = NetworkManager.Singleton.IsHost ? "Host" : "Client";
        }

        private void UpdateButtons()
        {
            _clientButton.gameObject.SetActive(false);
            _hostButton.gameObject.SetActive(false);
        }
    }
}