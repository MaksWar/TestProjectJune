using Cysharp.Threading.Tasks;
using Infrastructure.Services.SoundService;
using UnityEngine;

namespace Gameplay.Tips.GameplayTips
{
    public class SoundGameplayTip : BaseGameplayTip
    {
        private readonly ISoundService _soundService;
        private readonly string _soundKey;

        public SoundGameplayTip(
            ISoundService soundService,
            string soundKey
        )
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