using Cysharp.Threading.Tasks;
using Infrastructure.UI.HUD;

namespace Infrastructure.Factories
{
    public interface IUIFactory
    {
        UniTask<IHUDRoot> CreateHUD();
    }
}