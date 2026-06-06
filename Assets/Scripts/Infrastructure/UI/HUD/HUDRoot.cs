using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Infrastructure.UI.HUD
{
    public class HUDRoot : MonoBehaviour, IHUDRoot
    {
        [SerializeField] private CanvasGroup hudCanvasGroup;

        private HudType _currentHudType;

        public Transform Transform => transform;
        public HudType HudType => _currentHudType;

        public async UniTask InitializeAsync()
        {
            DisableCanvasGroup();
        }

        public void Show(HudType hudType)
        {
            EnableCanvasGroup();
            Hide();
            
            _currentHudType = hudType;
        }

        public void Hide()
        {
            _currentHudType = HudType.None;
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
        Lobby = 1,
        Shop = 2,
        Gallery = 4,
        NoAds = 8,
        Auction = 16,
        RoomCustomisation = 32,
        GameplayWin = 64,
        MyWorksCollection = 128,
        Gameplay = 256,
    }
}
