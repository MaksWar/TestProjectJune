using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Sprites;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
        [Header("Brush Feel")]
        [SerializeField, Range(0.001f, 0.5f)] private float brushRadius = 0.14f;
        [SerializeField, Range(0.001f, 0.2f)] private float brushSoftness = 0.04f;
        [SerializeField, Range(0f, 0.08f)] private float brushNoiseStrength;
        [SerializeField, Range(1f, 80f)] private float brushNoiseScale = 28f;
        [Header("Progress Fill")]
        [SerializeField] private bool smoothProgressFill = true;
        [SerializeField, Min(0f)] private float smoothProgressFillDuration = 0.15f;

        private const int MaxParts = 32;
        private const int MaxPathPoints = 256;

        private static readonly int PathPointsId = Shader.PropertyToID("_PathPoints");
        private static readonly int PartDataId = Shader.PropertyToID("_PartData");
        private static readonly int PartProgressId = Shader.PropertyToID("_PartProgress");
        private static readonly int PartCountId = Shader.PropertyToID("_PartCount");
        private static readonly int SpriteUvRectId = Shader.PropertyToID("_SpriteUvRect");
        private static readonly int SpriteMetricScaleId = Shader.PropertyToID("_SpriteMetricScale");
        private static readonly int BrushRadiusId = Shader.PropertyToID("_BrushRadius");
        private static readonly int BrushSoftnessId = Shader.PropertyToID("_BrushSoftness");
        private static readonly int BrushNoiseStrengthId = Shader.PropertyToID("_BrushNoiseStrength");
        private static readonly int BrushNoiseScaleId = Shader.PropertyToID("_BrushNoiseScale");

        private readonly List<TracePart> _parts = new();
        private readonly Vector4[] _pathPointArray = new Vector4[MaxPathPoints];
        private readonly Vector4[] _partDataArray = new Vector4[MaxParts];
        private readonly float[] _partProgressArray = new float[MaxParts];
        private readonly Tween[] _progressTweens = new Tween[MaxParts];

        private Material _material;
        private int _pathPointCount;
        private int _activePartIndex;

        private void Awake()
        {
            if (viewSpriteRenderer != null)
            {
                _material = viewSpriteRenderer.material;

                ApplySpriteUvRectToShader();
                ApplyBrushSettingsToShader();
            }
        }

        private void Start()
        {
            ApplySpriteUvRectToShader();
            ApplyBrushSettingsToShader();
            ApplyPartsToShader();
            UploadProgressToShader();
        }

        public void InitializeParts(IReadOnlyList<PathComponent> paths)
        {
            ClearParts();

            if (paths != null)
            {
                int partCount = Mathf.Min(paths.Count, MaxParts);

                if (paths.Count > MaxParts)
                {
                    Debug.LogWarning(
                        $"{nameof(LetterTracingController)} supports only {MaxParts} parts. Extra paths will not be painted.",
                        this);
                }

                for (int i = 0; i < partCount; i++)
                {
                    AddPart(paths[i]?.Path);
                }
            }

            _activePartIndex = 0;
            ApplySpriteUvRectToShader();
            ApplyBrushSettingsToShader();
            ApplyPartsToShader();
            UploadProgressToShader();
        }

        public void SetActivePart(int partIndex)
        {
            if (_parts.Count == 0)
            {
                _activePartIndex = 0;
                return;
            }

            _activePartIndex = Mathf.Clamp(partIndex, 0, _parts.Count - 1);
        }

        public void SetProgress(float progress) =>
            SetPartProgress(_activePartIndex, progress);

        public void SetPartProgress(int partIndex, float progress) =>
            SetPartProgress(partIndex, progress, smoothProgressFill);

        public void SetPartProgress(int partIndex, float progress, bool smooth)
        {
            if (partIndex < 0 || partIndex >= _parts.Count)
            {
                return;
            }

            float targetProgress = Mathf.Clamp01(progress);
            KillProgressTween(partIndex);

            if (smooth == false || smoothProgressFillDuration <= 0f || Application.isPlaying == false)
            {
                SetPartProgressImmediate(partIndex, targetProgress);
                return;
            }

            float currentProgress = _parts[partIndex].Progress;
            if (Mathf.Approximately(currentProgress, targetProgress))
            {
                SetPartProgressImmediate(partIndex, targetProgress);
                return;
            }

            Tween progressTween = null;
            progressTween = DOVirtual.Float(
                    currentProgress,
                    targetProgress,
                    smoothProgressFillDuration,
                    value => SetPartProgressImmediate(partIndex, value))
                .SetEase(Ease.OutQuad)
                .SetLink(gameObject)
                .OnKill(() =>
                {
                    if (_progressTweens[partIndex] == progressTween)
                    {
                        _progressTweens[partIndex] = null;
                    }
                });

            _progressTweens[partIndex] = progressTween;
        }

        private void SetPartProgressImmediate(int partIndex, float progress)
        {
            _parts[partIndex].Progress = progress;
            UploadProgressToShader();
        }

        public void ResetTracing() =>
            ResetAllParts();

        public void ResetAllParts()
        {
            KillAllProgressTweens();

            for (int i = 0; i < _parts.Count; i++)
            {
                _parts[i].Progress = 0f;
            }

            UploadProgressToShader();
        }

        public void CompletePart(int partIndex) =>
            SetPartProgress(partIndex, 1f);

        private void ClearParts()
        {
            KillAllProgressTweens();

            _parts.Clear();
            _pathPointCount = 0;

            Array.Clear(_pathPointArray, 0, _pathPointArray.Length);
            Array.Clear(_partDataArray, 0, _partDataArray.Length);
            Array.Clear(_partProgressArray, 0, _partProgressArray.Length);
        }

        private void KillProgressTween(int partIndex)
        {
            if (partIndex < 0 || partIndex >= _progressTweens.Length || _progressTweens[partIndex] == null)
            {
                return;
            }

            _progressTweens[partIndex].Kill();
            _progressTweens[partIndex] = null;
        }

        private void KillAllProgressTweens()
        {
            for (int i = 0; i < _progressTweens.Length; i++)
            {
                KillProgressTween(i);
            }
        }

        private void OnDestroy() =>
            KillAllProgressTweens();

        private void AddPart(IReadOnlyList<Vector2> localPath)
        {
            TracePart part = new()
            {
                StartIndex = _pathPointCount,
            };

            if (localPath != null)
            {
                int availablePointCount = MaxPathPoints - _pathPointCount;
                int pointCount = Mathf.Min(localPath.Count, availablePointCount);

                if (localPath.Count > availablePointCount)
                {
                    Debug.LogWarning(
                        $"{nameof(LetterTracingController)} supports only {MaxPathPoints} total points. Current path was truncated.",
                        this);
                }

                for (int i = 0; i < pointCount; i++)
                {
                    Vector2 uv = LocalPositionToUv(localPath[i]);
                    part.UvWaypoints.Add(uv);
                    _pathPointArray[_pathPointCount] = new Vector4(uv.x, uv.y, 0f, 0f);
                    _pathPointCount++;
                }
            }

            part.PointCount = part.UvWaypoints.Count;
            part.TotalLength = CalculateTotalLength(part.UvWaypoints, GetSpriteMetricScale());
            _parts.Add(part);
        }

        private void ApplyPartsToShader()
        {
            if (_material == null)
            {
                return;
            }

            Array.Clear(_partDataArray, 0, _partDataArray.Length);

            int partCount = Mathf.Min(_parts.Count, MaxParts);
            for (int i = 0; i < partCount; i++)
            {
                TracePart part = _parts[i];
                _partDataArray[i] = new Vector4(part.StartIndex, part.PointCount, part.TotalLength, 0f);
            }

            _material.SetVectorArray(PathPointsId, _pathPointArray);
            _material.SetVectorArray(PartDataId, _partDataArray);
            _material.SetInt(PartCountId, partCount);
        }

        private void ApplyBrushSettingsToShader()
        {
            if (_material == null)
            {
                return;
            }

            _material.SetFloat(BrushRadiusId, brushRadius);
            _material.SetFloat(BrushSoftnessId, brushSoftness);
            _material.SetFloat(BrushNoiseStrengthId, brushNoiseStrength);
            _material.SetFloat(BrushNoiseScaleId, brushNoiseScale);
        }

        private void ApplySpriteUvRectToShader()
        {
            if (_material == null)
            {
                return;
            }

            Vector4 spriteUvRect = new(0f, 0f, 1f, 1f);
            if (viewSpriteRenderer != null && viewSpriteRenderer.sprite != null)
            {
                spriteUvRect = DataUtility.GetOuterUV(viewSpriteRenderer.sprite);
            }

            _material.SetVector(SpriteUvRectId, spriteUvRect);
            _material.SetVector(SpriteMetricScaleId, GetSpriteMetricScale());
        }

        private void UploadProgressToShader()
        {
            if (_material == null)
            {
                return;
            }

            Array.Clear(_partProgressArray, 0, _partProgressArray.Length);

            int partCount = Mathf.Min(_parts.Count, MaxParts);
            for (int i = 0; i < partCount; i++)
            {
                _partProgressArray[i] = _parts[i].Progress;
            }

            _material.SetFloatArray(PartProgressId, _partProgressArray);
        }

        private Vector2 LocalPositionToUv(Vector2 localPosition)
        {
            if (viewSpriteRenderer == null || viewSpriteRenderer.sprite == null)
            {
                return Vector2.zero;
            }

            Sprite sprite = viewSpriteRenderer.sprite;
            Bounds bounds = sprite.bounds;

            if (bounds.size.x <= 0f || bounds.size.y <= 0f)
            {
                return Vector2.zero;
            }

            float normalizedU = (localPosition.x - bounds.min.x) / bounds.size.x;
            float normalizedV = (localPosition.y - bounds.min.y) / bounds.size.y;

            return new Vector2(normalizedU, normalizedV);
        }

        private Vector2 GetSpriteMetricScale()
        {
            if (viewSpriteRenderer == null || viewSpriteRenderer.sprite == null)
            {
                return Vector2.one;
            }

            Bounds bounds = viewSpriteRenderer.sprite.bounds;
            if (bounds.size.y <= 0f)
            {
                return Vector2.one;
            }

            return new Vector2(bounds.size.x / bounds.size.y, 1f);
        }

        private static float CalculateTotalLength(IReadOnlyList<Vector2> points, Vector2 metricScale)
        {
            if (points == null || points.Count < 2)
            {
                return 0f;
            }

            float totalLength = 0f;
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 from = Vector2.Scale(points[i], metricScale);
                Vector2 to = Vector2.Scale(points[i + 1], metricScale);
                totalLength += Vector2.Distance(from, to);
            }

            return totalLength;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            viewSpriteRenderer = GetComponent<SpriteRenderer>();

            if (!Application.isPlaying || PrefabUtility.IsPartOfPrefabAsset(gameObject))
            {
                return;
            }

            if (_material == null && viewSpriteRenderer != null)
            {
                _material = viewSpriteRenderer.material;
            }

            ApplySpriteUvRectToShader();
            ApplyPartsToShader();
            ApplyBrushSettingsToShader();
            UploadProgressToShader();
        }

        private void OnDrawGizmosSelected()
        {
            if (_parts == null || _parts.Count == 0)
            {
                return;
            }

            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null || spriteRenderer.sprite == null)
            {
                return;
            }

            Vector3 PathPointToWorld(Vector2 uv)
            {
                Vector2 localPosition = UvToLocalPosition(uv, spriteRenderer.sprite);
                return transform.TransformPoint(new Vector3(localPosition.x, localPosition.y, 0f));
            }

            for (int partIndex = 0; partIndex < _parts.Count; partIndex++)
            {
                TracePart part = _parts[partIndex];
                if (part.UvWaypoints.Count < 2)
                {
                    continue;
                }

                Gizmos.color = Color.HSVToRGB((partIndex * 0.17f) % 1f, 0.85f, 1f);
                for (int i = 0; i < part.UvWaypoints.Count - 1; i++)
                {
                    Gizmos.DrawLine(PathPointToWorld(part.UvWaypoints[i]), PathPointToWorld(part.UvWaypoints[i + 1]));
                }

                for (int i = 0; i < part.UvWaypoints.Count; i++)
                {
                    Gizmos.DrawSphere(PathPointToWorld(part.UvWaypoints[i]), 0.03f);
                }
            }
        }

        private static Vector2 UvToLocalPosition(Vector2 uv, Sprite sprite)
        {
            Bounds bounds = sprite.bounds;
            float x = bounds.min.x + uv.x * bounds.size.x;
            float y = bounds.min.y + uv.y * bounds.size.y;
            return new Vector2(x, y);
        }
#endif

        private sealed class TracePart
        {
            public readonly List<Vector2> UvWaypoints = new();
            public int StartIndex;
            public int PointCount;
            public float TotalLength;
            public float Progress;
        }
    }
}
