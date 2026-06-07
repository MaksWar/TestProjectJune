using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Infrastructure.UI.HUD
{
    public class HUDRoot : MonoBehaviour, IHUDRoot
    {
        [SerializeField] private CanvasGroup hudCanvasGroup;

        public Transform Transform => transform;

        public async UniTask InitializeAsync()
        {
            DisableCanvasGroup();
        }

        public void Show()
        {
            EnableCanvasGroup();
        }

        public void Hide()
        {
            DisableCanvasGroup();
        }

        private void DisableCanvasGroup() =>
            hudCanvasGroup.alpha = 0;
        
        private void EnableCanvasGroup() =>
            hudCanvasGroup.alpha = 1;
    }

    [Flags]
    public enum HudType
    {
        None = 0,
        LevelMenu = 1,
        Gameplay = 2,
    }
}
