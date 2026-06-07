using System.Collections.Generic;
using Infrastructure.Services.SoundService;

namespace Gameplay.Level.Presentation
{
    public static class PresentationSoundsMap
    {
        public static readonly Dictionary<FigureType, string> PresentationSoundByType = new()
        {
            [FigureType.Letter] = SoundKeys.FollowTraceLetter,
            [FigureType.Shape] = SoundKeys.FollowTraceNumberShape,
            [FigureType.Number] = SoundKeys.FollowTraceNumberShape,
        };
    }
}