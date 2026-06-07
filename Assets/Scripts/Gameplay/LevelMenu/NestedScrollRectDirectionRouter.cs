using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gameplay.LevelMenu
{
    public sealed class NestedScrollRectDirectionRouter : ScrollRect
    {
        [SerializeField] private ScrollRect parentScrollRect;
        [SerializeField] private float directionThreshold = 8f;

        private bool _directionResolved;
        private bool _routeToParent;
        private bool _childDragStarted;
        private bool _parentDragStarted;

        public void SetParentScrollRect(ScrollRect scrollRect)
        {
            parentScrollRect = scrollRect;
        }

        public override void OnInitializePotentialDrag(PointerEventData eventData)
        {
            _directionResolved = false;
            _routeToParent = false;
            _childDragStarted = false;
            _parentDragStarted = false;

            base.OnInitializePotentialDrag(eventData);
            parentScrollRect?.OnInitializePotentialDrag(eventData);
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            ResolveDirection(eventData);
            StartResolvedDrag(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            ResolveDirection(eventData);
            StartResolvedDrag(eventData);

            if (!_directionResolved)
            {
                return;
            }

            if (_routeToParent)
            {
                parentScrollRect?.OnDrag(eventData);
                return;
            }

            base.OnDrag(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (_parentDragStarted)
            {
                parentScrollRect?.OnEndDrag(eventData);
            }
            else if (_childDragStarted)
            {
                base.OnEndDrag(eventData);
            }

            _directionResolved = false;
            _routeToParent = false;
            _childDragStarted = false;
            _parentDragStarted = false;
        }

        private void ResolveDirection(PointerEventData eventData)
        {
            if (_directionResolved)
            {
                return;
            }

            Vector2 dragDelta = eventData.position - eventData.pressPosition;
            if (dragDelta.magnitude < directionThreshold)
            {
                return;
            }

            _routeToParent = Mathf.Abs(dragDelta.y) > Mathf.Abs(dragDelta.x);
            _directionResolved = true;
        }

        private void StartResolvedDrag(PointerEventData eventData)
        {
            if (!_directionResolved)
            {
                return;
            }

            if (_routeToParent)
            {
                if (!_parentDragStarted)
                {
                    parentScrollRect?.OnBeginDrag(eventData);
                    _parentDragStarted = true;
                }

                return;
            }

            if (!_childDragStarted)
            {
                base.OnBeginDrag(eventData);
                _childDragStarted = true;
            }
        }
    }
}
