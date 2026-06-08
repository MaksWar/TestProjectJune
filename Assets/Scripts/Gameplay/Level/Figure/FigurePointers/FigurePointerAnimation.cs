using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Gameplay.Level
{
    public class FigurePointerAnimationComponent : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer view;
        [Header("Setings")]
        [SerializeField, Range(0f, 1f)] private float startTransparency = 0f;
        [SerializeField, Range(0f, 1f)] private float targetTransparency = 1f;

        private Tween _showTween;

        public async UniTask Show(float duration)
        {
            KillShowTween();
            SetVisible(true);

            if (view == null)
            {
                return;
            }

            SetTransparency(startTransparency);

            if (duration <= 0f)
            {
                SetTransparency(targetTransparency);

                return;
            }

            UniTaskCompletionSource completionSource = new();
            Color targetColor = view.color;
            targetColor.a = targetTransparency;

            _showTween = view
                .DOColor(targetColor, duration)
                .SetEase(Ease.OutQuad)
                .SetLink(gameObject)
                .OnComplete(() => completionSource.TrySetResult())
                .OnKill(() => completionSource.TrySetResult());

            await completionSource.Task;

            if (_showTween != null && _showTween.IsActive() == false)
            {
                _showTween = null;
            }
        }

        public void Hide()
        {
            KillShowTween();
            SetTransparency(startTransparency);
            SetVisible(false);
        }

        private void SetTransparency(float transparency)
        {
            Color color = view.color;
            color.a = transparency;

            view.color = color;
        }

        private void KillShowTween()
        {
            if (_showTween == null)
            {
                return;
            }

            _showTween.Kill();
            _showTween = null;
        }

        private void SetVisible(bool isVisible) =>
            view.gameObject.SetActive(isVisible);
    }
}
