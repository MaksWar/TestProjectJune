using Cysharp.Threading.Tasks;
namespace Infrastructure.StaticData
{
    public interface IStaticDataService
    {
        UniTask LoadAllAsync();
    }
}
