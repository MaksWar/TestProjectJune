using Cysharp.Threading.Tasks;

namespace Gameplay.Tips.GameplayTips
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
