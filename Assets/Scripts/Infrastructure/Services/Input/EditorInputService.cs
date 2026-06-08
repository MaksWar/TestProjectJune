using UnityEngine;

namespace Infrastructure.Services.Input
{
    public class EditorInputService : InputService
    {
        protected override bool TryGetPointerState(out PointerInputState pointerState)
        {
            bool isPressed = UnityEngine.Input.GetMouseButton(0);
            bool wasPressedThisFrame = UnityEngine.Input.GetMouseButtonDown(0);
            bool wasReleasedThisFrame = UnityEngine.Input.GetMouseButtonUp(0);

            pointerState = new PointerInputState(
                UnityEngine.Input.mousePosition,
                isPressed,
                wasPressedThisFrame,
                wasReleasedThisFrame);

            return isPressed || wasPressedThisFrame || wasReleasedThisFrame;
        }
    }
}
