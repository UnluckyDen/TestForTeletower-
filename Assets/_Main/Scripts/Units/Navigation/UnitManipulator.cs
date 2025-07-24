using System;
using _Main.Scripts.Infrastructure;
using _Main.Scripts.Match;
using _Main.Scripts.Units.UnitCommands;
using Unity.Netcode;
using UnityEngine;

namespace _Main.Scripts.Units.Navigation
{
    public class UnitManipulator : MonoBehaviour
    {
        public event Action<UnitCommandData> UnitCommandGiven;
        
        [SerializeField] private ObjectSelector _objectSelector;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private LayerMask _groundLayerMask;
        
        private readonly RaycastHit[] _raycastHits = new RaycastHit[1];
        
        private void Awake() =>
            _mainCamera = Camera.main;

        private void Update()
        {
            if (!Input.GetMouseButtonDown(1))
                return;
            
            if (_objectSelector.SelectedObject is not BaseUnit unit)
                return;
            
            if (!MatchController.Instance.CanUseUnit(NetworkManager.Singleton.LocalClientId, unit.NetworkObjectId))
                return;
            
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

            int hitCount = Physics.RaycastNonAlloc(ray, _raycastHits, 100f, _groundLayerMask);
            

            if (hitCount > 0)
                SetPathToUnit(unit, _raycastHits[0].point);
        }

        private void SetPathToUnit(BaseUnit unit, Vector3 position)
        {
            UnitCommandGiven?.Invoke(new UnitCommandData(
                UnitCommandType.MoveCommand,
                unit.NetworkObjectId,
                position,
                0));
        }
    }
}