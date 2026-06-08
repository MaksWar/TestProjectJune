using System;
using System.Collections.Generic;
using Infrastructure.Services.Input;
using UnityEngine;
using Zenject;
using InputInteractable = Infrastructure.Services.Input.IInteractable;

namespace Gameplay.Level
{
    public class InteractionHandlerComponent : MonoBehaviour
    {
        private readonly HashSet<InputInteractable> _interactedObjects = new();

        private IInputService _inputService;
        private bool _isSubscribed;

        public event Action<IDraggingInteractable> Interacted;

        [Inject]
        private void Construct(IInputService inputService) =>
            _inputService = inputService;

        public void Initialize() =>
            Subscribe();

        private void OnDestroy() =>
            Unsubscribe();

        private void Subscribe()
        {
            if (_isSubscribed || _inputService == null)
            {
                return;
            }

            _inputService.Dragged += OnDragged;
            _inputService.Clicked += OnClicked;
            _inputService.DragEnded += OnDragEnded;
            _inputService.Released += OnReleased;
            _isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_isSubscribed || _inputService == null)
            {
                return;
            }

            _inputService.Dragged -= OnDragged;
            _inputService.Clicked -= OnClicked;
            _inputService.DragEnded -= OnDragEnded;
            _inputService.Released -= OnReleased;
            _isSubscribed = false;
            _interactedObjects.Clear();
        }

        private void OnClicked(InputPointerData inputData)
        {
            IDraggingInteractable interactable = inputData.Interactable as IDraggingInteractable;
            if (interactable == null)
            {
                return;
            }

            Interacted?.Invoke(interactable);
        }

        private void OnDragged(InputPointerData inputData)
        {
            IDraggingInteractable interactable = inputData.Interactable as IDraggingInteractable;
            if (interactable == null || !_interactedObjects.Add(interactable))
            {
                return;
            }

            Interacted?.Invoke(interactable);
        }

        private void OnDragEnded(InputPointerData inputData) =>
            _interactedObjects.Clear();

        private void OnReleased(InputPointerData inputData) =>
            _interactedObjects.Clear();
    }
}
