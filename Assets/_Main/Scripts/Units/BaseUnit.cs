using System;
using _Main.Scripts.Infrastructure.Interfaces;
using _Main.Scripts.Settings;
using _Main.Scripts.Units.StateMachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace _Main.Scripts.Units
{
    public class BaseUnit : NetworkBehaviour, IHoverable, ISelectable
    {
        [SerializeField] private UnitSettings _unitSettings;
        
        [SerializeField] private GameObject _hoveredGameObject;
        [SerializeField] private GameObject _selectedGameObject;
        [SerializeField] private MeshRenderer _meshRenderer;

        [Space]
        [SerializeField] private NavMeshAgent _navMeshAgent;
        
        private UnitStateMachine _unitStateMachine;
        
        public PlayerSide PlayerSide { get; private set; }

        private void Awake()
        {
            HoverExit();
            Deselect();
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
                UnitRegistry.Instance.RemoveUnit(this);
        }

        public void Init()
        {
            _unitStateMachine = new UnitStateMachine();
        }

        public void SetTarget(Vector3 target)
        {
            _navMeshAgent.SetDestination(target);
        }

        public void HoverEnter() => 
            _hoveredGameObject.SetActive(true);

        public void HoverExit() =>
            _hoveredGameObject.SetActive(false);

        public void Select() => 
            _selectedGameObject.SetActive(true);

        public void Deselect() =>
            _selectedGameObject.SetActive(false);

        [ClientRpc]
        public void UpdatePlayerSideClientRpc(PlayerSide playerSide) =>
            PlayerSide = playerSide;

        [ClientRpc]
        public void UpdateMaterialClientRpc(PlayerSide playerSide) =>
            _meshRenderer.material = _unitSettings.SideMaterials[playerSide];
    }
}
