using System;
using Modules.CameraSystem.CameraAnimations;
using UnityEngine;

namespace Modules.CameraSystem.CameraSystemCore
{
    internal interface ICameraController
    {
        AnimationType TargetAnimationType { get; }
        event Action AppliedStateToGameObject;
        void Enable(bool isOn);
        bool IsActive { get; }
        void Set(CameraAnimationProperty property, float value);
        void SetAnchor(Transform anchor);
        void ForceUpdate();
    }
}
