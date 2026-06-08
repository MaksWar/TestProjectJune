using UnityEngine;

namespace Infrastructure.Services.Input
{
    public class InputPointerData
    {
        public readonly Vector2 ScreenPosition;
        public readonly Vector2 WorldPosition;
        public readonly IInteractable Interactable;

        public InputPointerData(
            Vector2 screenPosition,
            Vector2 worldPosition,
            IInteractable interactable)
        {
            ScreenPosition = screenPosition;
            WorldPosition = worldPosition;
            Interactable = interactable;
        }
    }
}
