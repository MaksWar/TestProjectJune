using UnityEngine;

namespace Gameplay.Level
{
    [DisallowMultipleComponent]
    public class FigureCameraHeightFitter : MonoBehaviour
    {
        [SerializeField, Tooltip("World-unit padding. X = bottom padding, Y = top padding.")]
        private Vector2 verticalPadding;
        [SerializeField] private bool centerOnCamera = true;
        [SerializeField] private bool includeInactiveRenderers;

        private Camera _targetCamera;

        public void SetCamera(Camera cameraToFit) =>
            _targetCamera = cameraToFit;

        public void Fit()
        {
            Camera cameraToFit = _targetCamera;
            if (cameraToFit == null || !TryGetRenderersBounds(out Bounds bounds))
            {
                return;
            }

            float cameraHeight = GetCameraWorldHeight(cameraToFit);
            float targetHeight = cameraHeight - verticalPadding.x - verticalPadding.y;
            if (targetHeight <= 0f || bounds.size.y <= 0f)
            {
                return;
            }

            float scale = targetHeight / bounds.size.y;
            transform.localScale = new Vector3(
                transform.localScale.x * scale,
                transform.localScale.y * scale,
                transform.localScale.z);

            if (centerOnCamera)
            {
                CenterBoundsOnCamera(cameraToFit);
            }
        }

        private void CenterBoundsOnCamera(Camera cameraToFit)
        {
            if (!TryGetRenderersBounds(out Bounds bounds))
            {
                return;
            }

            Vector3 cameraPosition = cameraToFit.transform.position;
            Vector3 targetCenter = new(
                cameraPosition.x,
                cameraPosition.y,
                bounds.center.z);

            transform.position += targetCenter - bounds.center;
        }

        private bool TryGetRenderersBounds(out Bounds bounds)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(includeInactiveRenderers);
            bounds = default;
            bool hasBounds = false;

            foreach (Renderer childRenderer in renderers)
            {
                if (childRenderer == null || !childRenderer.enabled)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = childRenderer.bounds;
                    hasBounds = true;
                    continue;
                }

                bounds.Encapsulate(childRenderer.bounds);
            }

            return hasBounds;
        }

        private float GetCameraWorldHeight(Camera cameraToFit)
        {
            if (cameraToFit.orthographic)
            {
                return cameraToFit.orthographicSize * 2f;
            }

            float distance = Mathf.Abs(transform.position.z - cameraToFit.transform.position.z);
            return 2f * distance * Mathf.Tan(cameraToFit.fieldOfView * 0.5f * Mathf.Deg2Rad);
        }
    }
}
