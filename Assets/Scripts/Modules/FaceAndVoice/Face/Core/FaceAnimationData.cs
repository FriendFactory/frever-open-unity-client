using System.Collections.Generic;
using UnityEngine;

namespace Modules.FaceAndVoice.Face.Core
{
    public sealed class FaceAnimationData
    {
        public IReadOnlyCollection<FaceAnimFrame> Frames => _frames;
        private readonly List<FaceAnimFrame> _frames = new List<FaceAnimFrame>();

        private readonly Dictionary<BlendShape, AnimationCurve> _blendShapesAnimationCurves =
            new Dictionary<BlendShape, AnimationCurve>();
        
        public FaceAnimationData()
        {
        }
        
        public FaceAnimationData(ICollection<FaceAnimFrame> frames)
        {
            _frames.AddRange(frames);
            ResetAnimationCurves();
            
            foreach (var frame in _frames)
            {
                AddFrameToCurve(frame);
            }
        }

        public void AddFrame(FaceAnimFrame frame)
        {
            _frames.Add(frame);
            AddFrameToCurve(frame);
        }

        public float GetValueAtTime(BlendShape blendShape, float time)
        {
            if (_blendShapesAnimationCurves.TryGetValue(blendShape, out var animationCurve))
                return animationCurve.Evaluate(time);
            
            return 0;
        }

        private void AddFrameToCurve(FaceAnimFrame frame)
        {
            foreach (var blendShape in frame.BlendShapesData)
            {
                AddKeyFrameToCurve(blendShape.Key, frame.FrameTime ,blendShape.Value);
            }
        }
        
        private void AddKeyFrameToCurve(BlendShape blendShape, float time, float value)
        {
            if(_blendShapesAnimationCurves.TryGetValue(blendShape, out var animationCurve))
            {
                animationCurve.AddKey(time, value);
            }
            else
            {
                var curve = new AnimationCurve();
                curve.AddKey(time, value);
                _blendShapesAnimationCurves.Add(blendShape, curve);
            }
        }
        
        private void ResetAnimationCurves()
        {
            _blendShapesAnimationCurves.Clear();
        }
    }
}