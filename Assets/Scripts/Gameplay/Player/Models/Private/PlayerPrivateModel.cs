using Infrastructure.Services.SaveLoadSystem;

namespace Gameplay.MetaGameplay.Player.Models.Private
{
    public class PlayerPrivateModel : BasePrivateModel<PlayerPrivateScheme>
    {
        public override string DataName => "player";
        
        public int CurrentLevel => Data.CurrentLevel;
        public int SoundVolume => Data.SoundVolume;
        public int MusicVolume =>  Data.MusicVolume;
        public int HapticsVolume => Data.HapticsVolume;

        public override void Setup()
        {
            base.Setup();

            if (Data.CurrentLevel < 1)
            {
                SetLevel(1);
            }

            Data.HapticsVolume = 1;
            Data.MusicVolume = 1;
            Data.SoundVolume = 1;
        }

        public void SetMusicVolume(int volume)
        {
            Data.MusicVolume = volume;
            
            SetKeyDirty(PlayerPrivateScheme.MusicVolumeKey);
        }

        public void SetSoundVolume(int volume)
        {
            Data.SoundVolume = volume;
            
            SetKeyDirty(PlayerPrivateScheme.SoundsVolumeKey);
        }

        public void SetHapticVolume(int volume)
        {
            Data.HapticsVolume = volume;
            
            SetKeyDirty(PlayerPrivateScheme.HapticsVolumeKey);
        }

        public void SetLevel(int level)
        {
            Data.CurrentLevel = level;
            
            SetKeyDirty(PlayerPrivateScheme.CurrentLevelKey);
        }
    }
}