using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Infrastructure.Services.SoundService
{
    public interface ISoundService
    {
        int SoundVolume { get; }
        int MusicVolume { get; }
        
        UniTask InitializeAsync();
        void SetMusicVolume(int volume);
        void SetSoundVolume(int volume);
        UniTask<int> PlayMusicAsync(string soundKey, float volume = 1f, bool loop = false, bool persist = false,
            float fadeInSeconds = 1f, float fadeOutSeconds = 1f, float currentMusicFadeOutSeconds = -1f,
            Transform sourceTransform = null);
        UniTask<int> PlaySoundAsync(string soundKey, float volume = 1f, bool loop = false,
            Transform sourceTransform = null, CancellationToken cancellationToken = default);
        UniTask<int> PlayUISoundAsync(string soundKey, float volume = 1f, int maxSimultaneousCount = -1);
        void StopAll();
        void StopAllMusic(float fadeOutSeconds = -1f);
        void StopAllSounds();
        void StopAllUISounds();
        void PauseAll();
        void ResumeAll();
    }
}
