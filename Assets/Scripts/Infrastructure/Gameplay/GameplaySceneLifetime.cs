using System;
using System.Threading;

namespace Infrastructure.Gameplay
{
    public class GameplaySceneLifetime : IGameplaySceneLifetime, IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private bool _isCancellationRequested;

        public CancellationToken CancellationToken => _cancellationTokenSource.Token;
        public bool IsCancellationRequested => _isCancellationRequested || _cancellationTokenSource.IsCancellationRequested;

        public void Dispose()
        {
            if (_isCancellationRequested)
            {
                return;
            }

            _isCancellationRequested = true;
            _cancellationTokenSource.Cancel();
        }
    }
}
