using System;
using System.Collections.Generic;
using System.Linq;
using Modules.FaceAndVoice.Face.Core;
using Modules.FaceAndVoice.Face.Recording.Core;
using NUnit.Framework;

namespace Tests.EditMode
{
    internal sealed class FaceAnimationConvertingTests
    {
        [Test]
        public void ConvertFaceAnimation()
        {
            //arrange
            var originAnim = new FaceAnimationData();
            for (var i = 0; i < 10; i++)
            {
                var blendShapes = new Dictionary<BlendShape, float>
                {
                    {BlendShape.EyeBlinkRight, i}, 
                    {BlendShape.EyeBlinkLeft, i}
                };
                var frame = new FaceAnimFrame(i, blendShapes);
                originAnim.AddFrame(frame);
            }

            var blendShapesMap = new FaceBlendShapeMap();
            var converter = new FaceAnimationConverter(blendShapesMap);
            
            //act
            var animAsText = converter.ConvertToString(originAnim);
            var resultAnim = converter.ConvertToFaceAnimationData(animAsText);
            
            //assert
            Assert.True(resultAnim.Frames.Count == originAnim.Frames.Count);
            for (var frameIndex = 0; frameIndex < resultAnim.Frames.Count; frameIndex++)
            {
                var originFrame = originAnim.Frames.ElementAt(frameIndex);
                var resultFrame = resultAnim.Frames.ElementAt(frameIndex);
                
                Assert.True(originFrame.BlendShapesData.Count == resultFrame.BlendShapesData.Count);
                foreach (var blendShapesData in originFrame.BlendShapesData)
                {
                    Assert.True(resultFrame.BlendShapesData.ContainsKey(blendShapesData.Key));
                    Assert.True(Math.Abs(resultFrame.BlendShapesData[blendShapesData.Key] - blendShapesData.Value) < 0.000001f);
                }
            }
        }
    }
}
