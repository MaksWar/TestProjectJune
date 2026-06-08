using Cysharp.Threading.Tasks;

namespace Infrastructure.States
{
    public interface IPayloadedState<TPayload> : IExitableState
    {
        UniTask Enter(TPayload levelID);
    }
}