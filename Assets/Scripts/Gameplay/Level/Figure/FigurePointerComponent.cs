using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Gameplay.Level
{
    public class FigurePointerComponent : MonoBehaviour
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
    }
}
