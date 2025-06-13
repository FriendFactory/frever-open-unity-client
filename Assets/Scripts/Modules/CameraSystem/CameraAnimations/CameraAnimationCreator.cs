using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Modules.CameraSystem.PlayerCamera;

namespace Modules.CameraSystem.CameraAnimations
{
    [UsedImplicitly]
    internal sealed class CameraAnimationCreator
    {
        private static readonly CameraAnimationProperty[] RotationProperties =
            {CameraAnimationProperty.RotationX, CameraAnimationProperty.RotationY, CameraAnimationProperty.RotationZ};
        
        private readonly CinemachineBasedController _controller;
        private readonly CameraAnimationConverter _animationConverter;
        
        private readonly RotationCurve360FlipCorrector _curve360FlipCorrector;

        private readonly Dictionary<CameraAnimationProperty, IList<float>> _propertyToListMap;
        private readonly CameraAnimationSaver _saver;

        private readonly List<float> _time = new List<float>();

        public CameraAnimationCreator(CinemachineBasedController controller, CameraAnimationSaver saver,
            RotationCurve360FlipCorrector curve360FlipCorrector, CameraAnimationConverter animationConverter)
        {
            _controller = controller;
            _animationConverter = animationConverter;
            _saver = saver;
            
            _curve360FlipCorrector = curve360FlipCorrector;

            _propertyToListMap = new Dictionary<CameraAnimationProperty, IList<float>>();
            foreach (CameraAnimationProperty prop in Enum.GetValues(typeof(CameraAnimationProperty)))
            {
                _propertyToListMap.Add(prop, new List<float>());
            }
        }

        public void SaveValues(float time)
        {
            _time.Add(time);

            foreach (var keyPair in _propertyToListMap)
            {
                var animPropKey = keyPair.Key;
                var propValuesList = keyPair.Value;
                var val = _controller.GetValue(animPropKey);
                propValuesList.Add(val);
            }
        }

        public (string filePath, string animationString) ConstructTextFile(string fileName = null)
        {
            FixRotationCurves360Flip();
            
            var animationString = _animationConverter.ConvertToString(_time, _propertyToListMap);
            var filePath = _saver.SaveTextFileFromString(animationString, fileName);
            Reset();
            return (filePath, animationString);
        }

        public void Reset()
        {
            foreach (var propListValue in _propertyToListMap.Values)
            {
                propListValue.Clear();
            }
            _time.Clear();
        }

        private void FixRotationCurves360Flip()
        {
            var rotationCurves = _propertyToListMap.Where(x => RotationProperties.Contains(x.Key)).Select(x => x.Value);
            foreach (var curveValues in rotationCurves)
            {
                if (!Has360Flip(curveValues)) continue;

                FixRotationCurve360Flip(curveValues);
            }
        }

        private void FixRotationCurve360Flip(IList<float> curveValues)
        {
            _curve360FlipCorrector.FixFlip(curveValues);
        }

        private bool Has360Flip(IList<float> curveValues)
        {
            return _curve360FlipCorrector.Has360Flip(curveValues);
        }
    }
}