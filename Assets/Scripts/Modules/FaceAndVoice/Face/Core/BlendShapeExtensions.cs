#if UNITY_IOS
using UnityEngine.XR.ARKit;
#endif

namespace Modules.FaceAndVoice.Face.Core
{
    public static class BlendShapeExtensions
    {
#if UNITY_IOS
        public static BlendShape ToGlobalBlendShape(this ARKitBlendShapeLocation blendShapeLocation)
        {
            return (BlendShape) (int) (blendShapeLocation);
        }
#endif
    }
}