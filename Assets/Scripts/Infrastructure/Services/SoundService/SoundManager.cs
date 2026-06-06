using Cysharp.Threading.Tasks;
using Hellmade.Sound;
using Infrastructure.AssetManagement;
using UnityEngine;

namespace Infrastructure.Services.SoundService
{
    public class SoundManager
    {
        private const string SoundAssetFolder = "Sounds/";

        private readonly IAssetsProvider _assetsProvider;

        public SoundManager(IAssetsProvider assetsProvider)
        {
            _assetsProvider = assetsProvider;
        }

        public async UniTask<int> PlayMusicAsync(string soundKey, float volume = 1f, bool loop = false,
            bool persist = false, float fadeInSeconds = 1f, float fadeOutSeconds = 1f,
            float currentMusicFadeOutSeconds = -1f, Transform sourceTransform = null)
        {
            AudioClip clip = await LoadClipAsync(soundKey);
            if (clip == null)
            {
                return -1;
            }

            return EazySoundManager.PlayMusic(
                clip,
                volume,
                loop,
                persist,
                fadeInSeconds,
                fadeOutSeconds,
                currentMusicFadeOutSeconds,
                sourceTransform);
        }

        public async UniTask<int> PlaySoundAsync(string soundKey, float volume = 1f, bool loop = false,
            Transform sourceTransform = null)
        {
            AudioClip clip = await LoadClipAsync(soundKey);
            if (clip == null)
            {
                return -1;
            }

            return EazySoundManager.PlaySound(clip, volume, loop, sourceTransform);
        }

        public async UniTask<int> PlayUISoundAsync(string soundKey, float volume = 1f, int maxSimultaneousCount = -1)
        {
            AudioClip clip = await LoadClipAsync(soundKey);
            if (clip == null)
            {
                return -1;
            }

            if (maxSimultaneousCount > 0
                && EazySoundManager.CountPlayingUISounds(clip) >= maxSimultaneousCount)
            {
                return -1;
            }

            return EazySoundManager.PlayUISound(clip, volume);
        }

        public void StopAll() =>
            EazySoundManager.StopAll();

        public void StopAllMusic(float fadeOutSeconds = -1f) =>
            EazySoundManager.StopAllMusic(fadeOutSeconds);

        public void StopAllSounds() =>
            EazySoundManager.StopAllSounds();

        public void StopAllUISounds() =>
            EazySoundManager.StopAllUISounds();

        public void PauseAll() =>
            EazySoundManager.PauseAll();

        public void ResumeAll() =>
            EazySoundManager.ResumeAll();

        private async UniTask<AudioClip> LoadClipAsync(string soundKey)
        {
            string address = GetPath(soundKey);

            AudioClip clip = await _assetsProvider.Load<AudioClip>(address, GetType());
            if (clip == null)
            {
                Debug.LogError($"[SoundManager] Failed to load AudioClip for key '{soundKey}' by address '{address}'.");
            }

            return clip;
        }

        private static string GetPath(string soundKey) =>
            $"{SoundAssetFolder}{soundKey}.ogg";
    }
}
