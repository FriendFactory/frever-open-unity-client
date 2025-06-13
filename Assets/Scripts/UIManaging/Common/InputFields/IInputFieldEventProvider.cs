using System;
using Zenject;

namespace UIManaging.Common.InputFields
{
    public interface IInputFieldEventProvider
    {
        public event Action InputFieldActivated;
        public event Action InputFieldDeactivated;
        public event Action InputFieldSlidedDown;
    }
}