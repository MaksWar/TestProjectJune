using Cysharp.Threading.Tasks;
using Infrastructure.UI.GUI;
using Infrastructure.UI.HUD;

namespace Infrastructure.UI
{
    public interface IUIService
    {
        IHUDRoot HUDRoot { get; }
        IGUIRoot GUIRoot { get; }

        UniTask InitializeAsync();
        UniTask<TComponent> OpenUIEntity<TComponent>(string id, bool gui = true) where TComponent : class;
        void CloseUIEntity(string id, bool gui = true);
    }
}
