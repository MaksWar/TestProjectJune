using Cysharp.Threading.Tasks;

namespace Infrastructure.SceneMenegment
{
    public interface ISceneLoader
    {
        UniTask Load(string sceneName);
    }
}