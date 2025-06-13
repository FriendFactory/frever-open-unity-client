using System.Collections;
using System.Collections.Generic;

namespace Modules.CameraSystem.CameraAnimations
{
    public sealed class CameraAnimationFrame: IEnumerable<KeyValuePair<CameraAnimationProperty, float>>
    {
        private readonly IReadOnlyDictionary<CameraAnimationProperty, float> _frameValues;

        public CameraAnimationFrame(IReadOnlyDictionary<CameraAnimationProperty, float> frameValues)
        {
            _frameValues = frameValues;
        }
        
        public float GetValue(CameraAnimationProperty property)
        {
            return _frameValues[property];
        }

        public IEnumerator<KeyValuePair<CameraAnimationProperty, float>> GetEnumerator()
        {
            return _frameValues.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}