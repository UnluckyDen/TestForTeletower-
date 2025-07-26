using System;
using _Main.Scripts.Match;
using _Main.Scripts.Units.UnitCommands;
using Unity.Netcode;
using UnityEngine;
using ObjectSelector = _Main.Scripts.Infrastructure.ObjectSelector;

namespace _Main.Scripts.Units.Navigation
{
    public class UnitManipulator : MonoBehaviour
    {
        public event Action<UnitCommandData> UnitCommandGiven;

        [SerializeField] private ObjectSelector _objectSelector;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private LayerMask _manipulatorLayerMask;
        [SerializeField] private float _doubleClickThreshold = 0.3f;
        
        private float _lastClickTime = 0f;
        
        private Vector3 _lastClickedPoint;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(1))
                return;

            if (_objectSelector.SelectedObject is not BaseUnit unit)
                return;

            if (!MatchController.Instance.CanUseUnit(NetworkManager.Singleton.LocalClientId, unit.NetworkObjectId))
                return;

            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit, 100f, _manipulatorLayerMask))
                return;
            
            Vector3 clickedPoint = hit.point;
            float currentTime = Time.time;
            
            BaseUnit unitToAttack = hit.collider.GetComponent<BaseUnit>();
            
            if (unitToAttack != null)
            { 
                AttackUnit(unit, unitToAttack);
                return;
            }
                
            if (currentTime - _lastClickTime <= _doubleClickThreshold)
            {
                SetPathToUnit(unit, clickedPoint);
            }
            else
            {
                _lastClickTime = currentTime;
                _lastClickedPoint = clickedPoint;

                PredicatePath(unit, clickedPoint);
            }
        }

        private void PredicatePath(BaseUnit unit, Vector3 position)
        {
            unit?.ToDrawPathState(position);
        }

        private void SetPathToUnit(BaseUnit unit, Vector3 position)
        {
            if (!unit.CanReachPoint(position))
                return;
            
            unit.ClearVisualPath();
            UnitCommandGiven?.Invoke(new UnitCommandData(
                UnitCommandType.MoveCommand,
                unit.NetworkObjectId,
                position,
                0));
        }

        private void AttackUnit(BaseUnit unit, BaseUnit unitToAttack)
        {
            if (MatchController.Instance.CanAttackUnit(NetworkManager.Singleton.LocalClientId, unit.NetworkObjectId, unitToAttack.NetworkObjectId))
                UnitCommandGiven?.Invoke(new UnitCommandData(
                    UnitCommandType.AttackCommand,
                    unit.NetworkObjectId,
                    Vector3.zero,
                    unitToAttack.NetworkObjectId));
        }
    }
}
