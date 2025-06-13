using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.CameraSystem.CameraAnimations.Template.ScriptableObjects
{
    /// <summary>
    ///   Scriptable object for setup and store camera animation templates
    /// </summary>
    [CreateAssetMenu(order = 10, fileName = "CameraAnimationTemplate", menuName = "ScriptableObjects/CameraAnimationTemplate")]
    internal sealed class CameraAnimationTemplateData : ScriptableObject
    {
        //saved serialized data
        [SerializeField] private List<FrameInfo> _frames;

        public Dictionary<CameraAnimationProperty, AnimationCurve> AnimationCurves => 
            _animationCurves ?? (_animationCurves = BuildAnimationCurves());

        private Dictionary<CameraAnimationProperty, AnimationCurve> _animationCurves;
        
        private Dictionary<CameraAnimationProperty, AnimationCurve> BuildAnimationCurves()
        { 
            var curves = new Dictionary<CameraAnimationProperty, AnimationCurve>();
            foreach (var frame in _frames)
            {
                foreach (var cameraPropInfo in frame.Properties)
                {
                    var cameraProperty = cameraPropInfo.Property;
                    var cameraPropValue = cameraPropInfo.Value;
                    
                    if(!curves.ContainsKey(cameraProperty))
                        curves.Add(cameraProperty, new AnimationCurve());

                    curves[cameraProperty].AddKey(frame.Time, cameraPropValue);
                }
            }
            return curves;
        }
        
        [Serializable]
        private struct FrameInfo
        {
            public float Time;
            public List<CameraProperty> Properties;
        }
        
        [Serializable]
        private struct CameraProperty
        {
            public CameraAnimationProperty Property;
            public float Value;
        }
    }
}
