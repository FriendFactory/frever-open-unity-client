using System;
using UnityEngine;

namespace Modules.InputHandling
{
    public interface IInputManager: IRotationGestureSource
    {
        bool Enabled { get; }
        event Action<bool> EnableStateChangeRequested; 
        event Action<float> PanHorizontalExecuting;
        event Action<float> PanVerticalExecuting;
        event Action<float> ZoomExecuting;
        event Action ZoomBegin;
        event Action PanHorizontalBegin;
        event Action PanVerticalBegin;
        event Action ZoomEnd;
        event Action PanHorizontalEnd;
        event Action PanVerticalEnd;
        void Enable(bool enable, params GestureType[] gestureTypes);
    }

    public interface IRotationGestureSource
    {
        event Action<Vector2> RotationBegan;
        event Action<float> RotationExecuted;
        event Action RotationEnded;
    }

    public enum GestureType
    {
        Pan,
        Zoom,
        Rotation
    }
}
