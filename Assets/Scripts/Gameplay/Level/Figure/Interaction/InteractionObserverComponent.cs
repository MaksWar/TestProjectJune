using UnityEngine;

namespace Gameplay.Level
{
    public class InteractionObserverComponent : MonoBehaviour, IDraggingInteractable
    {
        [SerializeField] private Collider2D interactionCollider;

        public void Activate() =>
            interactionCollider.enabled = true;

        public void Deactivate() =>
            interactionCollider.enabled = false;
    }
}
