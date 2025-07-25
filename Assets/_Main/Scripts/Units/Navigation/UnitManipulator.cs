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
        [SerializeField] private LayerMask _groundLayerMask;

        private readonly RaycastHit[] _raycastHits = new RaycastHit[1];

        private float _lastClickTime = 0f;
        private float _doubleClickThreshold = 0.3f;
        
        private Vector3 _lastClickedPoint;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(1)) return;

            if (_objectSelector.SelectedObject is not BaseUnit unit) return;

            if (!MatchController.Instance.CanUseUnit(NetworkManager.Singleton.LocalClientId, unit.NetworkObjectId)) return;

            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            int hitCount = Physics.RaycastNonAlloc(ray, _raycastHits, 100f, _groundLayerMask);

            if (hitCount == 0) return;

            Vector3 clickedPoint = _raycastHits[0].point;
            float currentTime = Time.time;

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
            unit?.DrawPath(position);
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
    }
}
