using System;
using UnityEngine;

namespace Gameplay.Level
{
    [Obsolete("Use Infrastructure.Services.Input.IInteractable instead.")]
    public interface IInteractable
    {
        GameObject GameObject { get; }
    }
}
