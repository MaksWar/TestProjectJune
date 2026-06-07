using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Gameplay.Level.Figure.PaintShader
{
    /// <summary>
    /// Drives the LetterTracing shader from external gameplay progress.
    /// Input and validation are owned by LevelService / pointer interaction.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class LetterTracingController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer viewSpriteRenderer;
        [Header("Tracing Path")]
        [SerializeField] private bool waypointsAreLocalPositions = true;

        private Material _material;
        private List<Vector2> _waypoints = new();

        private float _currentProgress;

        private const int MaxWaypoints = 64;

        private static readonly int ProgressId = Shader.PropertyToID("_Progress");
        private static readonly int WaypointsId = Shader.PropertyToID("_Waypoints");
        private static readonly int WaypointCountId = Shader.PropertyToID("_WaypointCount");

        private void Awake() =>
            _material = viewSpriteRenderer.material;

        private void Start()
        {
            ApplyWaypointsToShader();
            SetProgress(0f);
        }

        public void InitializePath(IReadOnlyList<Vector2> localPath)
        {
            _waypoints.Clear();

            if (localPath != null)
            {
                int count = Mathf.Min(localPath.Count, MaxWaypoints);
                for (int i = 0; i < count; i++)
                {
                    _waypoints.Add(LocalPositionToUv(localPath[i]));
                }
            }

            waypointsAreLocalPositions = false;
            ApplyWaypointsToShader();
            ResetTracing();
        }

        public void SetProgress(float progress)
        {
            _currentProgress = Mathf.Clamp01(progress);

            if (_material != null)
            {
                _material.SetFloat(ProgressId, _currentProgress);
            }
        }

        public void ResetTracing()
        {
            SetProgress(0f);
        }

        public void CompleteTracing() =>
            SetProgress(1f);

        private void ApplyWaypointsToShader()
        {
            if (_material == null)
            {
                return;
            }

            int count = Mathf.Min(_waypoints.Count, MaxWaypoints);
            Vector4[] waypointArray = new Vector4[MaxWaypoints];

            for (int i = 0; i < count; i++)
            {
                Vector2 uv = waypointsAreLocalPositions ? LocalPositionToUv(_waypoints[i]) : _waypoints[i];
                waypointArray[i] = new Vector4(uv.x, uv.y, 0f, 0f);
            }

            _material.SetVectorArray(WaypointsId, waypointArray);
            _material.SetInt(WaypointCountId, count);
        }

        private Vector2 LocalPositionToUv(Vector2 localPosition)
        {
            if (viewSpriteRenderer == null || viewSpriteRenderer.sprite == null)
            {
                return Vector2.zero;
            }

            Bounds bounds = viewSpriteRenderer.sprite.bounds;
            float u = (localPosition.x - bounds.min.x) / bounds.size.x;
            float v = (localPosition.y - bounds.min.y) / bounds.size.y;

            return new Vector2(u, v);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            viewSpriteRenderer = GetComponent<SpriteRenderer>();

            if (!Application.isPlaying)
            {
                return;
            }

            ApplyWaypointsToShader();
            SetProgress(_currentProgress);
        }

        private void OnDrawGizmosSelected()
        {
            if (_waypoints == null || _waypoints.Count < 2)
            {
                return;
            }

            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null || spriteRenderer.sprite == null)
            {
                return;
            }

            Bounds bounds = spriteRenderer.sprite.bounds;

            Vector3 PathPointToWorld(Vector2 point)
            {
                Vector2 localPosition = waypointsAreLocalPositions ? point : UvToLocalPosition(point, bounds);
                return transform.TransformPoint(new Vector3(localPosition.x, localPosition.y, 0f));
            }

            Gizmos.color = Color.green;
            for (int i = 0; i < _waypoints.Count - 1; i++)
            {
                Gizmos.DrawLine(PathPointToWorld(_waypoints[i]), PathPointToWorld(_waypoints[i + 1]));
            }

            for (int i = 0; i < _waypoints.Count; i++)
            {
                Gizmos.color = i == 0 ? Color.cyan : i == _waypoints.Count - 1 ? Color.red : Color.yellow;
                Gizmos.DrawSphere(PathPointToWorld(_waypoints[i]), 0.03f);
            }
        }

        private static Vector2 UvToLocalPosition(Vector2 uv, Bounds bounds)
        {
            float x = bounds.min.x + uv.x * bounds.size.x;
            float y = bounds.min.y + uv.y * bounds.size.y;
            return new Vector2(x, y);
        }
#endif
    }
}
