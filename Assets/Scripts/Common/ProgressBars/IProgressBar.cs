using System;

namespace Common.ProgressBars
{
    public interface IProgressBar
    {
        event Action<float> ValueChanged;
        float Value { get; set; }
        
        float Min { get; }
        float Max { get; }
    }
}