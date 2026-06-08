using Cysharp.Threading.Tasks;

namespace Infrastructure.UI.LoadingCurtain.Proxy
{
    public interface ILoadingCurtainProxy
    {
        UniTask InitializeAsync();

        void Show();

        void Hide();
    }
}