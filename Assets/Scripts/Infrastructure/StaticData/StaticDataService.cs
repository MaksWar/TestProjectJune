using System;
using Cysharp.Threading.Tasks;

namespace Infrastructure.StaticData
{
    public class StaticDataService : IStaticDataService
    {
        public UniTask LoadAllAsync()
        {

            return UniTask.CompletedTask;
        }
    }
}
