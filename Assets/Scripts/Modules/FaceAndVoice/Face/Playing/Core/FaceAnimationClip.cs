using System.Linq;
using Modules.FaceAndVoice.Face.Core;

namespace Modules.FaceAndVoice.Face.Playing.Core
{
    public sealed class FaceAnimationClip
    {
        public float Duration { get; set; }
        public string FullSavePath { get; set; }
        public string RelativePath { get; set; }
        
        public FaceAnimationData FaceAnimationData { get; private set; }

        public float TimeCurveStartValue => FaceAnimationData.Frames.First().FrameTime;
        
        public FaceAnimationClip()
        {
            FaceAnimationData = new FaceAnimationData();
        }

        public FaceAnimationClip(FaceAnimationData faceAnimationData)
        {
            FaceAnimationData = faceAnimationData;
        }

        public void AddFrame(FaceAnimFrame frame)
        {
            FaceAnimationData.AddFrame(frame);
        }
        
        public void Clear()
        {
            FaceAnimationData = null;
        }

        public float GetValueAtTime(BlendShape blendShape, float time)
        {
            return FaceAnimationData.GetValueAtTime(blendShape, time);
        }
        
    }
}
