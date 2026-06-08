using Cysharp.Threading.Tasks;

namespace Infrastructure.Services.SaveLoadSystem
{
    public interface ISaveLoadManager
    {
        UniTask InitializeAsync();
    }
}