namespace Utilities.Pool
{
    public interface IPoolableObject
    {
        void OnPop();
        void OnPush();
    }
}