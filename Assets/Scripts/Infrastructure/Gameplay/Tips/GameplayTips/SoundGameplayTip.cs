using Cysharp.Threading.Tasks;
using Infrastructure.Services.SoundService;
using UnityEngine;

namespace Infrastructure.Gameplay.Tips
{
    public class SoundGameplayTip : BaseGameplayTip
    {
        private readonly ISoundService _soundService;
        private readonly string _soundKey;

        public SoundGameplayTip(float inactivityTime, ISoundService soundService, string soundKey) : base(inactivityTime)
        {
            _soundService = soundService;
            _soundKey = soundKey;
        }

        public override UniTask Play(GameplayTipContext context)
        {
            if (string.IsNullOrWhiteSpace(_soundKey))
            {
                Debug.LogWarning($"{nameof(SoundGameplayTip)}: no sound key for figure type '{context.FigureType}'.");
                return UniTask.CompletedTask;
            }

            _soundService.PlaySoundAsync(_soundKey).Forget();

            return UniTask.CompletedTask;
        }
    }
}
