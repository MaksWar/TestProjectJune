using System.Collections.Generic;
using Infrastructure.Services.SaveLoadSystem;

namespace Gameplay.MetaGameplay.Player.Models.Private
{
    public class PlayerPrivateScheme : BasePrivateModelScheme
    {
        public int MusicVolume;
        public int SoundVolume;
        
        public const string MusicVolumeKey = "MusicVolume";
        public const string SoundsVolumeKey = "SoundsVolume";
        
        protected override Dictionary<string, object> SerializeProperties()
        {
            return new Dictionary<string, object>()
            {
                [MusicVolumeKey] = MusicVolume,
                [SoundsVolumeKey] = SoundVolume
            };
        }

        protected override void DeserializeProperties(Dictionary<string, object> data)
        {
            MusicVolume = GetValue<int>(MusicVolumeKey, data);
            SoundVolume = GetValue<int>(SoundsVolumeKey, data);
        }
    }
}