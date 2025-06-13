using System;

namespace Modules.FaceAndVoice.Face.Facade
{
    public interface IArSessionManager
    {
        bool IsActive { get; }
        event Action<bool> StateSwitched;
        void SetARActive(bool isActive);
    }
}