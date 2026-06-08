using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Infrastructure.UI.HUD
{
    public interface IHUDRoot
    {
        Transform Transform { get; }
        
        UniTask InitializeAsync();
        void Show();
        void Hide();
    }
}