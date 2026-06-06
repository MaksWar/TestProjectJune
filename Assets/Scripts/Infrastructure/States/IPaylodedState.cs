using Cysharp.Threading.Tasks;

namespace Infrastructure.States
{
    public interface IPaylodedState<TPayload> : IExitableState
    {
        UniTask Enter(TPayload voxelModelId);
    }
}