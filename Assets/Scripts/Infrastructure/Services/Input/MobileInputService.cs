using UnityEngine;

namespace Infrastructure.Services.Input
{
    public class MobileInputService : InputService
    {
        protected override bool TryGetPointerState(out PointerInputState pointerState)
        {
            if (UnityEngine.Input.touchCount == 0)
            {
                pointerState = default;
                return false;
            }

            Touch touch = UnityEngine.Input.GetTouch(0);
            bool wasReleasedThisFrame = touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
            bool wasPressedThisFrame = touch.phase == TouchPhase.Began;
            bool isPressed = !wasReleasedThisFrame;

            pointerState = new PointerInputState(
                touch.position,
                isPressed,
                wasPressedThisFrame,
                wasReleasedThisFrame);

            return true;
        }
    }
}
