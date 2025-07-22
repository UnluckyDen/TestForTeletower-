using System;
using _Main.Scripts.Infrastructure.Interfaces;
using UnityEngine;

namespace _Main.Scripts.Infrastructure
{
    public class ObjectSelector : MonoBehaviour
    {
        public event Action<ISelectable> SelectedObjectChanged;
        
        [SerializeField] private LayerMask _unitLayerMask;
        [SerializeField] private Camera _mainCamera;

        private ISelectable _selectedObject;
        private IHoverable _hoveredObject;
        
        private readonly RaycastHit[] _raycastHits = new RaycastHit[1];

        public ISelectable SelectedObject => _selectedObject;
        public IHoverable HoverableObject => HoverableObject;

        private void Awake() => 
            _mainCamera = Camera.main;

        private void Update()
        {
            HandleMouseHover();
            if (Input.GetMouseButtonDown(0))
                TrySelect();
        }

        private void HandleMouseHover()
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

            int hitCount = Physics.RaycastNonAlloc(ray, _raycastHits, 100f, _unitLayerMask);

            if (hitCount > 0)
            {
                IHoverable hoverable = _raycastHits[0].collider.GetComponent<IHoverable>();

                if (hoverable != null)
                {
                    HoverEnter(hoverable);
                    return;
                }
            }

            HoverExit();
        }

        private void TrySelect()
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

            int hitCount = Physics.RaycastNonAlloc(ray, _raycastHits, 100f, _unitLayerMask);

            if (hitCount > 0)
            {
                ISelectable selectable = _raycastHits[0].collider.GetComponent<ISelectable>();

                if (selectable != null)
                {
                    Select(selectable);
                    return;
                }
            }

            Deselect();
        }

        private void HoverEnter(IHoverable hoverable)
        {
            _hoveredObject = hoverable;
            _hoveredObject.HoverEnter();
        }

        private void HoverExit()
        {
            if (_hoveredObject != null)
            {
                _hoveredObject.HoverExit();
                _hoveredObject = null;
            }
        }
        
        private void Select(ISelectable selectable)
        {
            if (_selectedObject != null)
                Deselect();
            
            _selectedObject = selectable;
            _selectedObject.Select();
            
            SelectedObjectChanged?.Invoke(_selectedObject);
        }

        private void Deselect()
        {
            if (_selectedObject != null)
            {
                _selectedObject.Deselect();
                _selectedObject = null;
                
                SelectedObjectChanged?.Invoke(_selectedObject);
            }
        }
    }
}