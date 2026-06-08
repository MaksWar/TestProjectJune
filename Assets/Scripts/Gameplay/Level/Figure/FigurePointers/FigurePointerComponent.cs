using Cysharp.Threading.Tasks;
using UnityEngine;
using Utilities.Pool;

namespace Gameplay.Level
{
    public class FigurePointerComponent : MonoBehaviour, IPoolableObject
    {
        [SerializeField] private PointerType figurePointerType;
        [SerializeField] private InteractionObserverComponent interactionObserverComponent;
        [SerializeField] private FigurePointerAnimationComponent pointerAnimationComponent;

        public PointerType FigurePointerType => figurePointerType;
        public IDraggingInteractable DraggingInteractable => interactionObserverComponent;

        public UniTask Show(float duration) =>
            pointerAnimationComponent.Show(duration);

        public void Hide() =>
            pointerAnimationComponent?.Hide();

        public void Activate() =>
            interactionObserverComponent?.Activate();

        public void Deactivate() =>
            interactionObserverComponent?.Deactivate();

        public void OnPop()
        {
            Hide();
            Deactivate();
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        public void OnPush()
        {
            Hide();
            Deactivate();
            transform.SetParent(null, false);
        }
    }
}
