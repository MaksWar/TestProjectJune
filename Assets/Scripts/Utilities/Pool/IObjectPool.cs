using Cysharp.Threading.Tasks;

namespace Utilities.Pool
{
    public interface IObjectPool<T> where T : IPoolableObject
    {
        UniTask<T> Pop(string path);
        void Push(T item);
        void Preload(string path, int count);
    }
}