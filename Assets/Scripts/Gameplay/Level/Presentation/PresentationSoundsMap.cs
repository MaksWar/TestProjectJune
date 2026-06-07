using System.Collections.Generic;
using Infrastructure.Services.SoundService;

namespace Gameplay.Level.Presentation
{
    public static class PresentationSoundsMap
    {
        public static readonly Dictionary<FigureType, string> StartPresentationSoundByType = new()
        {
            [FigureType.Letter] = SoundKeys.FollowTraceLetter,
            [FigureType.Shape] = SoundKeys.FollowTraceNumberShape,
            [FigureType.Number] = SoundKeys.FollowTraceNumberShape,
        };

        public static readonly List<string> WinSounds = new()
        {
            SoundKeys.Excellent,
            SoundKeys.Awesome,
            SoundKeys.ThatsGood
        };
    }
}