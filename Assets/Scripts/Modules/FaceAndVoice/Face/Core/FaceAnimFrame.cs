using System.Collections.Generic;

namespace Modules.FaceAndVoice.Face.Core
{
    public struct FaceAnimFrame
    {
        public readonly float FrameTime;
        public readonly Dictionary<BlendShape, float> BlendShapesData;

        public FaceAnimFrame(float frameTime, Dictionary<BlendShape, float> blendShapesData)
        {
            FrameTime = frameTime;
            BlendShapesData = blendShapesData;
        }
    }
}