using Cysharp.Threading.Tasks;
using Gameplay.MetaGameplay.Player.Models.Private;
using Hellmade.Sound;
using Infrastructure.AssetManagement;
using Infrastructure.Services.SaveLoadSystem;
using UnityEngine;

namespace Infrastructure.Services.SoundService
{
    public class SoundService : ISoundService
    {
        private readonly IPrivateModelProvider _privateModelProvider;
        private readonly SoundManager _soundManager;
        
        private PlayerPrivateModel _playerPrivateModel;

        public int SoundVolume => _playerPrivateModel.SoundVolume;
        public int MusicVolume => _playerPrivateModel.MusicVolume;

        public SoundService(IPrivateModelProvider privateModelProvider, IAssetsProvider assetsProvider)
        {
            _privateModelProvider = privateModelProvider;
            _soundManager = new SoundManager(assetsProvider);
        }

        public async UniTask InitializeAsync()
        {
            _playerPrivateModel = _privateModelProvider.Get<PlayerPrivateModel>();
            
            InitializeManager();
        }
        
        public void SetMusicVolume(int volume)
        {
            _playerPrivateModel.SetMusicVolume(volume);
            
            EazySoundManager.GlobalMusicVolume = volume;
        }

        public void SetSoundVolume(int volume)
        {
            _playerPrivateModel.SetSoundVolume(volume);
            
            EazySoundManager.GlobalSoundsVolume = volume;
            EazySoundManager.GlobalUISoundsVolume = volume;
        }

        public async UniTask<int> PlayMusicAsync(string soundKey, float volume = 1f, bool loop = false,
            bool persist = false, float fadeInSeconds = 1f, float fadeOutSeconds = 1f,
            float currentMusicFadeOutSeconds = -1f, Transform sourceTransform = null)
            => await _soundManager.PlayMusicAsync(soundKey, volume, loop, persist, fadeInSeconds, fadeOutSeconds,
                currentMusicFadeOutSeconds, sourceTransform);

        public async UniTask<int> PlaySoundAsync(string soundKey, float volume = 1f, bool loop = false,
            Transform sourceTransform = null) =>
            await _soundManager.PlaySoundAsync(soundKey, volume, loop, sourceTransform);

        public async UniTask<int> PlayUISoundAsync(string soundKey, float volume = 1f, int maxSimultaneousCount = -1) =>
            await _soundManager.PlayUISoundAsync(soundKey, volume, maxSimultaneousCount);

        public void StopAll() =>
            _soundManager.StopAll();

        public void StopAllMusic(float fadeOutSeconds = -1f) =>
            _soundManager.StopAllMusic(fadeOutSeconds);

        public void StopAllSounds() =>
            _soundManager.StopAllSounds();

        public void StopAllUISounds() =>
            _soundManager.StopAllUISounds();

        public void PauseAll() =>
            _soundManager.PauseAll();

        public void ResumeAll() =>
            _soundManager.ResumeAll();

        private void InitializeManager()
        {
            EazySoundManager.GlobalSoundsVolume = _playerPrivateModel.SoundVolume;
            EazySoundManager.GlobalUISoundsVolume = _playerPrivateModel.SoundVolume;
            EazySoundManager.GlobalMusicVolume = _playerPrivateModel.MusicVolume;
        }
    }
}
