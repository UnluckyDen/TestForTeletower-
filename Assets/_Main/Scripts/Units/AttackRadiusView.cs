using System.Collections.Generic;
using _Main.Scripts.Settings;
using UnityEngine;

namespace _Main.Scripts.Units
{
    public class AttackRadiusView : MonoBehaviour
    {
        [SerializeField] private Transform _rangeViewTransform;
        [SerializeField] private LayerMask _attackLayerMask;
        
        private PlayerSide _playerSide;
        private float _attackRange;

        private List<BaseUnit> _unistOnCheck = new();
        private Collider[] _results = new Collider[20];

        private bool _active;
        
        public void SetAttackRangeSettings(float range, PlayerSide playerSide)
        {
            _attackRange = range;
            _playerSide = playerSide;
            _rangeViewTransform.localScale = new Vector3(_attackRange * 2, _rangeViewTransform.localScale.y, _attackRange * 2);
        }

        private void OnDestroy()
        {
            CheckZone(false);
        }

        private void Update()
        {
            if (_active)
                CheckZone(_active);
        }

        public void SetZoneActive(bool active)
        {
            _active = active;
            transform.gameObject.SetActive(active);
        }

        private void CheckZone(bool active)
        {
            _active = true;
            
            foreach (var baseUnit in _unistOnCheck)
            {
                if (baseUnit != null)
                    baseUnit.SetOnAttack(false);
            }

            _unistOnCheck.Clear();

            if (active)
            {
                Physics.OverlapSphereNonAlloc(transform.position, _attackRange, _results, _attackLayerMask);

                foreach (var collider in _results)
                {
                    if (collider == null)
                        return;
                    
                    BaseUnit unit = collider.GetComponentInParent<BaseUnit>();
                    if (unit != null && unit.PlayerSide != _playerSide)
                    {
                        unit.SetOnAttack(true);
                        _unistOnCheck.Add(unit);
                    }
                }
            }
        }
    }
}