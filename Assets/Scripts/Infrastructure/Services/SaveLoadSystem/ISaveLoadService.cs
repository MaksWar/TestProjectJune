using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Infrastructure.Services.SaveLoadSystem
{
    public interface ISaveLoadService
    {
        UniTask SaveAsync(Dictionary<string,object> data, CancellationToken cancellationToken = default);
        UniTask<Dictionary<string, object>> LoadAsync(CancellationToken cancellationToken = default);
    }
}