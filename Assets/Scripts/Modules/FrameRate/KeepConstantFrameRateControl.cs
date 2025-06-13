using JetBrains.Annotations;
using UnityEngine;

namespace Modules.FrameRate
{
    [UsedImplicitly]
    internal sealed class KeepConstantFrameRateControl: IFrameRateControl
    {
        private readonly int _targetFrameRate;

        public KeepConstantFrameRateControl(int targetFrameRate)
        {
            _targetFrameRate = targetFrameRate;
        }

        public void Initialize()
        {
            Application.targetFrameRate = _targetFrameRate;
        }
    }
}