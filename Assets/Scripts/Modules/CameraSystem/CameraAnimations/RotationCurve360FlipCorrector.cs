using System.Collections.Generic;
using UnityEngine;

namespace Modules.CameraSystem.CameraAnimations
{
    /// <summary>
    ///     Transform provides rotation (euler angles) values in range [0,360].
    ///     If one frame was 359 and next one just 1 (truly speaking 361), we will have wrong curve evaluation in that range
    /// </summary>
    internal sealed class RotationCurve360FlipCorrector
    {
        private const float FRAME_ANGLE_DIFFERENCE_FLIP_INDICATOR = 300;//if difference between frames > 300, it must be flip
        
        public bool Has360Flip(IList<float> curveValues)
        {
            for (var i = 0; i < curveValues.Count - 1; i++)
            {
                var currentValue = curveValues[i];
                var nextValue = curveValues[i + 1];
                if (Is360FlipFrames(currentValue, nextValue))
                    return true;
            }

            return false;
        }
        
        public void FixFlip(IList<float> curveValues)
        {
            float flipCorrection = 0;
            for (var i = 0; i < curveValues.Count-1; i++)
            {
                var nextIndex = i + 1;
                var nextSupposedValue = curveValues[nextIndex] + flipCorrection;
                var currentValue = curveValues[i];

                var isFlip = Is360FlipFrames(currentValue, nextSupposedValue);

                if (!isFlip)
                {
                    curveValues[i + 1] = nextSupposedValue;
                    continue;
                }

                var nextValue = nextSupposedValue;
                var flipDirection = currentValue > nextValue ? FlipDirection.From360To0 : FlipDirection.From0To360;
                flipCorrection += 360 * (int) flipDirection;
                
                curveValues[nextIndex] += flipCorrection;
            }
        }
        
        private bool Is360FlipFrames(float previousFrameValue, float nextFrameValue)
        {
            var difference = Mathf.Abs(previousFrameValue - nextFrameValue);
            return difference > FRAME_ANGLE_DIFFERENCE_FLIP_INDICATOR;
        }
        
        private enum FlipDirection
        {
            From0To360 = -1,
            From360To0 =  1
        }
    }
}