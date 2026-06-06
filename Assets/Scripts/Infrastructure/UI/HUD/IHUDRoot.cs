using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Infrastructure.UI.HUD
{
    public interface IHUDRoot
    {
        Transform Transform { get; }
        HudType HudType { get; }
        
        UniTask InitializeAsync();
        void Show(HudType hudType);
        void Hide();
    }
}