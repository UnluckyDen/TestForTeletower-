using System.Collections;
using _Main.Scripts.Infrastructure.Interfaces;
using _Main.Scripts.Settings;
using _Main.Scripts.Units.UnitsStateMachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace _Main.Scripts.Units
{
    public class BaseUnit : NetworkBehaviour, IHoverable, ISelectable
    {
        [SerializeField] private SideSettings _unitSettings;
        [SerializeField] private float _unitSpeed;

        [SerializeField] private GameObject _hoveredGameObject;
        [SerializeField] private GameObject _selectedGameObject;
        [SerializeField] private MeshRenderer _meshRenderer;

        [Space]
        [SerializeField] private NavMeshAgent _navMeshAgent;
        [SerializeField] private NavMeshObstacle _navMeshObstacle;

        [SerializeField] private UnitPathPredictor _unitPathPredictor;

        private UnitStateMachine _unitStateMachine;

        public bool IsMoving => _unitStateMachine.CurrentState is MoveState;
        public float Speed => _unitSpeed;
        public float AttackRange => 0f;

        public PlayerSide PlayerSide { get; private set; }

        private void Awake()
        {
            HoverExit();
            Deselect();

            _unitStateMachine = new UnitStateMachine(this, _navMeshAgent, _navMeshObstacle, _unitPathPredictor);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            UnitRegistry.Instance.AddUnit(this);
        }


        public override void OnNetworkDespawn()
        {
            if (IsServer)
                UnitRegistry.Instance.RemoveUnit(this);
        }

        public void ToIdleState() => 
            _unitStateMachine.ToIdleState();

        public void ToMoveState(Vector3 target) =>
            _unitStateMachine.ToMoveState(target);

        public void ToAttackState(BaseUnit unitToAttack) =>
            _unitStateMachine.ToAttackState(unitToAttack);

        public void ToDrawPathState(Vector3 target) =>
            _unitStateMachine.ToDrawPathState(target);

        public bool CanReachPoint(Vector3 target) =>
            CalculatePathLength(CalculatePath(target)) <= _unitSpeed;

        public void ClearVisualPath()
        {
            _unitPathPredictor.ClearPath();
        }

        public void HoverEnter() =>
            _hoveredGameObject.SetActive(true);

        public void HoverExit() =>
            _hoveredGameObject.SetActive(false);

        public void Select()
        {
            _selectedGameObject.SetActive(true);
        }

        public void Deselect()
        { 
            _selectedGameObject.SetActive(false);
            _unitPathPredictor.ClearPath();
        }

        [ClientRpc]
        public void UpdatePlayerSideClientRpc(PlayerSide playerSide) =>
            PlayerSide = playerSide;

        [ClientRpc]
        public void UpdateMaterialClientRpc(PlayerSide playerSide) =>
            _meshRenderer.material = _unitSettings.SideMaterials[playerSide];

        public Vector3[] CalculatePath(Vector3 target)
        {
            NavMeshPath path = new NavMeshPath();
            if (!_navMeshAgent.CalculatePath(target, path) || path.status != NavMeshPathStatus.PathComplete)
                return null;

            return path.corners;
        }

        private float CalculatePathLength(Vector3[] path)
        {
            if (path == null || path.Length == 0)
                return -1f;
            
            float length = 0f;
            for (int i = 1; i < path.Length; i++)
                length += Vector3.Distance(path[i - 1], path[i]);

            return length;
        }
    }
}