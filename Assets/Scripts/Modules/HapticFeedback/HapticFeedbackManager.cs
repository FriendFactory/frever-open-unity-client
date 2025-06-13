using System;
using CandyCoded.HapticFeedback;
using JetBrains.Annotations;
using Settings;

namespace Modules.Haptics
{
    public enum HapticFX
    {
        Low = 0,
        Medium = 1,
        Heavy = 2,
    }

    public interface IHapticFeedbackManager
    {
        void PlayEffect(HapticFX effectType);
    }

    [UsedImplicitly]
    internal sealed class HapticFeedbackManager : IHapticFeedbackManager
    {
        public void PlayEffect(HapticFX effectType)
        {
            if (!AppSettings.HapticsEnabled) return;
            
            switch (effectType)
            {
                case HapticFX.Low:
                    HapticFeedback.LightFeedback();
                    break;
                case HapticFX.Medium:
                    HapticFeedback.MediumFeedback();
                    break;
                case HapticFX.Heavy:
                    HapticFeedback.HeavyFeedback();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(effectType), effectType, null);
            }
            
        }
    }
}
