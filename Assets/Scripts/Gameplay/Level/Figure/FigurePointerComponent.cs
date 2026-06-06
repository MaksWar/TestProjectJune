using UnityEngine;

namespace Gameplay.Level
{
    public class FigurePointerComponent : MonoBehaviour
    {
        [SerializeField] private GameObject viewObject;
        [SerializeField] private PointerType figurePointerType;
        [SerializeField] private InteractionObserverComponent interactionObserverComponent;

        public PointerType FigurePointerType => figurePointerType;
        public IDraggingInteractable DraggingInteractable => interactionObserverComponent;

        public void Show() =>
            SetVisible(true);

        public void Hide() =>
            SetVisible(false);

        public void Activate() =>
            interactionObserverComponent?.Activate();

        public void Deactivate() =>
            interactionObserverComponent?.Deactivate();

        private void SetVisible(bool isVisible) =>
            viewObject.SetActive(isVisible);
    }
}
