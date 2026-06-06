using System.Collections.Generic;
using Infrastructure.Services.SaveLoadSystem;

namespace Gameplay.MetaGameplay.Player.Models.Private
{
    public class PlayerPrivateScheme : BasePrivateModelScheme
    {
        public int MusicVolume;
        public int SoundVolume;
        public int CurrentLevel;
        public int HapticsVolume;

        public const string MusicVolumeKey = "MusicVolume";
        public const string SoundsVolumeKey = "SoundsVolume";
        public const string CurrentLevelKey = "currentLevel";
        public const string HapticsVolumeKey = "HapticsVolume";
        protected override Dictionary<string, object> SerializeProperties()
        {
            return new Dictionary<string, object>()
            {
                [CurrentLevelKey] = CurrentLevel,
                [MusicVolumeKey] = MusicVolume,
                [SoundsVolumeKey] = SoundVolume,
                [HapticsVolumeKey] = HapticsVolume,
            };
        }

        protected override void DeserializeProperties(Dictionary<string, object> data)
        {
            CurrentLevel = GetValue<int>(CurrentLevelKey, data);
            MusicVolume = GetValue<int>(MusicVolumeKey, data);
            SoundVolume = GetValue<int>(SoundsVolumeKey, data);
            HapticsVolume = GetValue<int>(HapticsVolumeKey, data);
        }
    }
}