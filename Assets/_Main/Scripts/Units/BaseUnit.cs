using _Main.Scripts.Infrastructure.Interfaces;
using _Main.Scripts.Settings;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace _Main.Scripts.Units
{
    public class BaseUnit : NetworkBehaviour, IHoverable, ISelectable
    {
        [SerializeField] private SideSettings _unitSettings;
        
        [SerializeField] private GameObject _hoveredGameObject;
        [SerializeField] private GameObject _selectedGameObject;
        [SerializeField] private MeshRenderer _meshRenderer;

        [Space]
        [SerializeField] private NavMeshAgent _navMeshAgent;
        
        public bool IsMoving => _navMeshAgent.hasPath 
                                && _navMeshAgent.remainingDistance > _navMeshAgent.stoppingDistance;
        
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

        public void SetTargetPoint(Vector3 target)
        {
            _navMeshAgent.SetDestination(target);
        }

        public void SetAttackUnit(BaseUnit unitToAttack)
        {
            
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
