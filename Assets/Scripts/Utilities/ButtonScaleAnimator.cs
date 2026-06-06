using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Utilities
{
    public class ButtonScaleAnimator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Transform target;
        [SerializeField] private bool useOriginalScaleOne = false;

        private Tween _scaleTween;
        private Vector3 _defaultScale;

        private const float PressedScale = 0.8f;
        private const float AnimationDurationDown = 0.08f;
        private const float AnimationDurationUp = 0.3f;
        private const Ease DownEase = Ease.Linear;
        private const Ease UpEase = Ease.OutBounce;

        private void Awake() =>
            _defaultScale = useOriginalScaleOne ? Vector3.one : target.localScale;

        public void OnPointerDown(PointerEventData eventData) =>
            DownAnimation(_defaultScale * PressedScale);

        public void OnPointerUp(PointerEventData eventData) =>
            UpAnimation(_defaultScale);

        private void DownAnimation(Vector3 targetScale)
        {
            KillTween();

            _scaleTween = target
                .DOScale(targetScale, AnimationDurationDown)
                .SetEase(DownEase);
        }
        
        private void UpAnimation(Vector3 targetScale)
        {
            KillTween();

            _scaleTween = target
                .DOScale(targetScale, AnimationDurationUp)
                .SetEase(UpEase);
        }

        private void KillTween()
        {
            if (_scaleTween == null)
            {
                return;
            }

            _scaleTween.Kill();
            _scaleTween = null;
        }

        #region Editor

        private void OnValidate()
        {
            if (target == null)
            {
                target = transform;
            }
        }

        #endregion
    }
}