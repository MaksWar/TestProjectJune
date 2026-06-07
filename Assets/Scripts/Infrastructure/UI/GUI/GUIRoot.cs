using UnityEngine;

namespace Infrastructure.UI.GUI
{
    public class GUIRoot : MonoBehaviour, IGUIRoot
    {
        [SerializeField] private CanvasGroup guiCanvasGroup;

        public Transform Transform => transform;
    }
}
