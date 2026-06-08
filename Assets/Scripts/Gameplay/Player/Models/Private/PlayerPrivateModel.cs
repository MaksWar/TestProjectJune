using Infrastructure.Services.SaveLoadSystem;

namespace Gameplay.MetaGameplay.Player.Models.Private
{
    public class PlayerPrivateModel : BasePrivateModel<PlayerPrivateScheme>
    {
        public override string DataName => "player";
        
        public int SoundVolume => Data.SoundVolume;
        public int MusicVolume =>  Data.MusicVolume;

        public override void Setup()
        {
            base.Setup();

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
    }
}