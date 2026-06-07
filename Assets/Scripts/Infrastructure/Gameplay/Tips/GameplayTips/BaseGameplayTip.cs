using Cysharp.Threading.Tasks;

namespace Infrastructure.Gameplay.Tips
{
    public abstract class BaseGameplayTip
    {
        protected BaseGameplayTip(float inactivityTime)
        {
            InactivityTime = inactivityTime;
        }

        public float InactivityTime { get; }

        public abstract UniTask Play(GameplayTipContext context);

        public virtual void Stop()
        {
        }
    }
}
