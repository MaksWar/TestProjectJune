using System;
using UnityEngine;

namespace Infrastructure.Services.Input
{
    public interface IInputService
    {
        event Action<InputPointerData> Pressed;
        event Action<InputPointerData> Released;
        event Action<InputPointerData> Clicked;
        event Action<InputPointerData> DragStarted;
        event Action<InputPointerData> Dragged;
        event Action<InputPointerData> DragEnded;

        bool IsPressed { get; }
        Vector2 ScreenPosition { get; }
        Vector2 WorldPosition { get; }
    }
}
