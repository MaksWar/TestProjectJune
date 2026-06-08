using Cysharp.Threading.Tasks;

namespace Gameplay.Tips.GameplayTips
{
    public abstract class BaseGameplayTip
    {
        public abstract UniTask Play(GameplayTipContext context);

        public virtual void Stop()
        {
        }
    }
}
