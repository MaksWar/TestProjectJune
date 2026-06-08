using System.Threading;

namespace Infrastructure.Gameplay
{
    public interface IGameplaySceneLifetime
    {
        CancellationToken CancellationToken { get; }
        bool IsCancellationRequested { get; }
    }
}
