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
        [SerializeField] private float _attackRange;
        [SerializeField] private LayerMask _attackLayerMask;

        [SerializeField] private GameObject _hoveredGameObject;
        [SerializeField] private GameObject _selectedGameObject;
        [SerializeField] private GameObject _onAttackGameObject;
        [SerializeField] private MeshRenderer _meshRenderer;

        [Space]
        [SerializeField] private NavMeshAgent _navMeshAgent;
        [SerializeField] private NavMeshObstacle _navMeshObstacle;

        [SerializeField] private UnitPathPredictor _unitPathPredictor;
        [SerializeField] private AttackRadiusView _attackRadiusView;

        private UnitStateMachine _unitStateMachine;

        public bool Selected { get; private set; }
        public bool IsMoving => _unitStateMachine.CurrentState is MoveState;
        public bool IsAttacking => _unitStateMachine.CurrentState is AttackState;
        public bool ReadyCalculatePath => _navMeshAgent.enabled;
        public float Speed => _unitSpeed;
        public float AttackRange => _attackRange;

        public PlayerSide PlayerSide { get; private set; }

        private void Awake()
        {
            HoverExit();
            Deselect();
            SetOnAttack(false);
            
            _unitStateMachine = new UnitStateMachine(this, _navMeshAgent, _navMeshObstacle, _unitPathPredictor, _attackRadiusView);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            UnitRegistry.Instance.AddUnit(this);
        }


        public override void OnNetworkDespawn()
        {
            UnitRegistry.Instance.RemoveUnit(this);
            base.OnNetworkDespawn();
        }

        public void ToIdleState() => 
            _unitStateMachine.ToIdleState();

        public void ToMoveState(Vector3 target) =>
            _unitStateMachine.ToMoveState(target);

        public void ToAttackState(BaseUnit unitToAttack) =>
            _unitStateMachine.ToAttackState(unitToAttack);

        public void ToDrawPathState(Vector3 target) =>
            _unitStateMachine.ToDrawPathState(target);

        public void ToCalculateState() =>
            _unitStateMachine.ToCalculatePathState();

        public bool CanReachPoint(Vector3 target)
        {
            float pathLength = CalculatePathLength(CalculatePath(target));
            if (Mathf.Approximately(pathLength, -1))
                return false;

            return pathLength <= _unitSpeed;
        }

        public bool CanAttackReachUnit(BaseUnit targetUnit)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, _attackRange, _attackLayerMask);

            foreach (var collider in colliders)
            {
                BaseUnit unit = collider.GetComponentInParent<BaseUnit>();
                if (unit != null && unit == targetUnit)
                    return true;
            }

            return false;
        }

        public bool CanAttackReachAnyUnitSide(PlayerSide targetSide)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, _attackRange, _attackLayerMask);

            foreach (var collider in colliders)
            {
                BaseUnit unit = collider.GetComponentInParent<BaseUnit>();
                if (unit != null && unit.PlayerSide == targetSide)
                    return true;
            }

            return false;
        }

        public void ClearVisualPath()
        {
            _unitPathPredictor.ClearPath();
            _attackRadiusView.transform.localPosition = Vector3.zero;
        }

        public void HoverEnter() =>
            _hoveredGameObject.SetActive(true);

        public void SetOnAttack(bool isOnAttack) =>
            _onAttackGameObject.SetActive(isOnAttack);

        public void HoverExit()
        {
            if (_hoveredGameObject == null)
                return;
            _hoveredGameObject.SetActive(false);
        }

        public void Select()
        {
            _selectedGameObject.SetActive(true);
            _attackRadiusView.SetZoneActive(true);
            Selected = true;
        }

        public void Deselect()
        { 
            if (_selectedGameObject == null)
                return;

            Selected = false;
            _attackRadiusView.SetZoneActive(false);
            _selectedGameObject.SetActive(false);
            ClearVisualPath();
        }

        [ClientRpc]
        public void UpdatePlayerSideClientRpc(PlayerSide playerSide)
        {
            PlayerSide = playerSide;
            _attackRadiusView.SetAttackRangeSettings(_attackRange, PlayerSide);
        }

        [ClientRpc]
        public void UpdateMaterialClientRpc(PlayerSide playerSide) =>
            _meshRenderer.material = _unitSettings.SideMaterials[playerSide];
        
        [ClientRpc]
        public void UpdateSpeedClientRpc(float speed) =>
            _unitSpeed = speed;

        public Vector3[] CalculatePath(Vector3 target)
        {
            NavMeshPath path = new NavMeshPath();
            if (_navMeshAgent.enabled == false 
                || !_navMeshAgent.CalculatePath(target, path)
                || path.status != NavMeshPathStatus.PathComplete)
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