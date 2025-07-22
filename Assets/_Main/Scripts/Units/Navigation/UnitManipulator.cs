using _Main.Scripts.Infrastructure;
using Unity.Netcode;
using UnityEngine;

namespace _Main.Scripts.Units.Navigation
{
    public class UnitManipulator : NetworkBehaviour
    {
        [SerializeField] private ObjectSelector _objectSelector;
        
        [SerializeField] private Camera _mainCamera;
        
        [SerializeField] private LayerMask _groundLayerMask;
        
        private readonly RaycastHit[] _raycastHits = new RaycastHit[1];
        
        private void Awake() =>
            _mainCamera = Camera.main;

        private void Update()
        {
            if (!IsOwner)
                return;
            
            if (!Input.GetMouseButtonDown(1))
                return;
            
            if (_objectSelector.SelectedObject is not BaseUnit unit)
                return;
            
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

            int hitCount = Physics.RaycastNonAlloc(ray, _raycastHits, 100f, _groundLayerMask);

            if (hitCount > 0)
                SetPathToUnit(unit, _raycastHits[0].point);
        }

        private void SetPathToUnit(BaseUnit unit, Vector3 position)
        {
            ulong networkObjectId = unit.NetworkObject.NetworkObjectId;
            SetTargetToUnitServerRpc(networkObjectId, position);
        }

        [ServerRpc]
        private void SetTargetToUnitServerRpc(ulong unitId, Vector3 position)
        {
            if (UnitRegistry.Instance.TryGetUnit(unitId, out BaseUnit unit))
            {
                unit.SetTarget(position);
            }
            else
            {
                Debug.LogWarning($"[Server] Unit {unitId} не найден в UnitRegistry");
            }
        }
    }
}