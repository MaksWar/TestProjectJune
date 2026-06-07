using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Infrastructure.Gameplay.Tips
{
    public class FingerComponent : MonoBehaviour
    {
        private Tween _moveTween;

        public void PlayRoute(
            IReadOnlyList<Vector2> route,
            Transform parent,
            float pointsPerSecond,
            Vector3 offset)
        {
            Stop();

            if (route == null || route.Count == 0 || parent == null)
            {
                return;
            }

            transform.SetParent(parent, false);
            gameObject.SetActive(true);
            SetPosition(route[0], offset);

            if (route.Count == 1)
            {
                return;
            }

            _moveTween = transform
                .DOLocalPath(CreateLocalPath(route, offset), GetMoveDuration(route.Count, pointsPerSecond), PathType.Linear, PathMode.Ignore)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart)
                .SetLink(gameObject);
        }

        public void Stop()
        {
            KillMoveTween();
            gameObject.SetActive(false);
        }

        private void OnDestroy() =>
            KillMoveTween();

        private void SetPosition(Vector2 position, Vector3 offset) =>
            transform.localPosition = (Vector3)position + offset;

        private static Vector3[] CreateLocalPath(IReadOnlyList<Vector2> route, Vector3 offset)
        {
            Vector3[] path = new Vector3[route.Count];
            for (int i = 0; i < route.Count; i++)
            {
                path[i] = (Vector3)route[i] + offset;
            }

            return path;
        }

        private static float GetMoveDuration(int pointsCount, float pointsPerSecond) =>
            Mathf.Max(0.01f, (pointsCount - 1) / Mathf.Max(0.01f, pointsPerSecond));

        private void KillMoveTween()
        {
            if (_moveTween == null)
            {
                return;
            }

            _moveTween.Kill();
            _moveTween = null;
        }
    }
}
