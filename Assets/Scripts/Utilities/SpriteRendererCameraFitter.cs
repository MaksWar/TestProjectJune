using UnityEngine;

namespace Utilities
{
    [RequireComponent(typeof(SpriteRenderer))]
    [DisallowMultipleComponent]
    public class SpriteRendererCameraFitter : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private bool centerOnCamera = true;
        [SerializeField] private Vector2 padding;

        private void OnEnable()
        {
            Fit();
        }

        public void Fit()
        {
            Vector2 cameraSize = GetCameraWorldSize(targetCamera);
            Vector2 spriteSize = spriteRenderer.bounds.size;

            if (spriteSize.x <= 0f || spriteSize.y <= 0f)
            {
                return;
            }

            float scale = Mathf.Max(
                (cameraSize.x + padding.x * 2f) / spriteSize.x,
                (cameraSize.y + padding.y * 2f) / spriteSize.y);

            transform.localScale = new Vector3(
                transform.localScale.x * scale,
                transform.localScale.y * scale,
                transform.localScale.z);

            if (centerOnCamera)
            {
                Vector3 cameraPosition = targetCamera.transform.position;
                transform.position = new Vector3(cameraPosition.x, cameraPosition.y, transform.position.z);
            }
        }

        private Vector2 GetCameraWorldSize(Camera cameraToFit)
        {
            if (cameraToFit.orthographic)
            {
                float height = cameraToFit.orthographicSize * 2f;
                return new Vector2(height * cameraToFit.aspect, height);
            }

            float distance = Mathf.Abs(transform.position.z - cameraToFit.transform.position.z);
            float heightAtDistance = 2f * distance * Mathf.Tan(cameraToFit.fieldOfView * 0.5f * Mathf.Deg2Rad);

            return new Vector2(heightAtDistance * cameraToFit.aspect, heightAtDistance);
        }
    }
}
