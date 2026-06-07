using System;
using UnityEngine;
using Zenject;

namespace Infrastructure.Services.Input
{
    public abstract class InputService : IInputService, ITickable
    {
        private const float DragStartDistance = 12f;

        private Camera _camera;
        private Vector2 _pressScreenPosition;
        private Vector2 _screenPosition;
        private Vector2 _worldPosition;
        private bool _isPressed;
        private bool _isDragging;
        private bool _isEnabled = true;

        public event Action<InputPointerData> Pressed;
        public event Action<InputPointerData> Released;
        public event Action<InputPointerData> Clicked;
        public event Action<InputPointerData> DragStarted;
        public event Action<InputPointerData> Dragged;
        public event Action<InputPointerData> DragEnded;

        public bool IsEnabled => _isEnabled;
        public bool IsPressed => _isPressed;
        public Vector2 ScreenPosition => _screenPosition;
        public Vector2 WorldPosition => _worldPosition;

        [Inject]
        private void Construct(Camera inputCamera) =>
            _camera = inputCamera;

        public void Tick()
        {
            if (!_isEnabled)
            {
                return;
            }

            if (!TryGetPointerState(out PointerInputState pointerState))
            {
                return;
            }

            _screenPosition = pointerState.ScreenPosition;
            _worldPosition = ScreenToWorld(_screenPosition);

            if (pointerState.WasPressedThisFrame)
            {
                _isPressed = true;
                _isDragging = false;
                _pressScreenPosition = _screenPosition;
                Pressed?.Invoke(CreatePointerData());
            }

            if (_isPressed && pointerState.IsPressed)
            {
                UpdateDragState();
            }

            if (pointerState.WasReleasedThisFrame && _isPressed)
            {
                InputPointerData pointerData = CreatePointerData();

                if (_isDragging)
                {
                    DragEnded?.Invoke(pointerData);
                }
                else
                {
                    Clicked?.Invoke(pointerData);
                }

                Released?.Invoke(pointerData);
                _isPressed = false;
                _isDragging = false;
            }
        }

        public void Enable() =>
            _isEnabled = true;

        public void Disable()
        {
            _isEnabled = false;
            _isPressed = false;
            _isDragging = false;
        }

        protected abstract bool TryGetPointerState(out PointerInputState pointerState);

        private void UpdateDragState()
        {
            float distanceFromPress = Vector2.Distance(_pressScreenPosition, _screenPosition);
            if (!_isDragging && distanceFromPress >= DragStartDistance)
            {
                _isDragging = true;
                DragStarted?.Invoke(CreatePointerData());
            }

            if (_isDragging)
            {
                Dragged?.Invoke(CreatePointerData());
            }
        }

        private Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            if (_camera == null)
            {
                return screenPosition;
            }

            return _camera.ScreenToWorldPoint(screenPosition);
        }

        private InputPointerData CreatePointerData()
        {
            Collider2D hitCollider = Physics2D.OverlapPoint(_worldPosition);
            IInteractable interactable = hitCollider != null
                ? hitCollider.GetComponent<IInteractable>()
                : null;

            return new InputPointerData(
                _screenPosition,
                _worldPosition,
                interactable);
        }

        protected readonly struct PointerInputState
        {
            public readonly Vector2 ScreenPosition;
            public readonly bool IsPressed;
            public readonly bool WasPressedThisFrame;
            public readonly bool WasReleasedThisFrame;

            public PointerInputState(
                Vector2 screenPosition,
                bool isPressed,
                bool wasPressedThisFrame,
                bool wasReleasedThisFrame)
            {
                ScreenPosition = screenPosition;
                IsPressed = isPressed;
                WasPressedThisFrame = wasPressedThisFrame;
                WasReleasedThisFrame = wasReleasedThisFrame;
            }
        }
    }
}
