using System;
using System.Collections.Generic;
using System.Linq;
using Modules.CameraSystem.CameraAnimations;
using Modules.CameraSystem.CameraAnimations.Template;
using NUnit.Framework;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Tests.EditMode
{
    internal sealed class NewCameraAnimationStartFrameDeltaTest
    {
        private const string CIRCULAR_ANIM_TEMPLATE_TXT = "0.000|0.000|0.500|4.000|4.000|55.000|0.000\n360.000|180.000|0.500|4.000|4.000|55.000|0.000";
        private static readonly CameraAnimationConverter CameraAnimationConverter = new CameraAnimationConverter();
        
        [Test]
        [TestCase(360, 0, -200, -560)]
        [TestCase(-180, 0, 180, 360)]
        [TestCase(180, 0, 180, 0)]
        [TestCase(-360, -200, -560, -400)]
        public void StartAnimationFromNewFrame_ShouldUpdateXAxisProperly(float firstFrameXAxis, float lastFrameXAxis, float startFrameXAxis, float expectedLastFrameXAxis)
        {
            var firstFrameString = $"0.000|0.000|0.000|0.000|0.000|0.000|0.000|55.000|0.000|0.000|{firstFrameXAxis}|0.500|4.000|4.000|0.000\n";
            var lastFrameString = $"360.000|0.000|0.000|0.000|0.000|0.000|0.000|55.000|0.000|0.000|{lastFrameXAxis}|0.500|4.000|4.000|0.000";
            var animationTxt = $"{firstFrameString}{lastFrameString}";
            var animation = CreateTestTemplate(animationTxt);
            
            var newStartFrameValues = new Dictionary<CameraAnimationProperty, float>
            {
                {CameraAnimationProperty.AxisX, startFrameXAxis},
                {CameraAnimationProperty.AxisY, 0.5f},
                {CameraAnimationProperty.OrbitRadius, 4f},
                {CameraAnimationProperty.HeightRadius, 4f},
                {CameraAnimationProperty.FieldOfView, 40},
                {CameraAnimationProperty.Dutch, 0},
                {CameraAnimationProperty.DepthOfField, 0},
                {CameraAnimationProperty.PositionX, 0},
                {CameraAnimationProperty.PositionY, 0},
                {CameraAnimationProperty.PositionZ, 0},
                {CameraAnimationProperty.RotationX, 0},
                {CameraAnimationProperty.RotationY, 0},
                {CameraAnimationProperty.RotationZ, 0},
                {CameraAnimationProperty.FocusDistance, 0}
            };

            var startFrame = CreateFrame(newStartFrameValues);

            animation.StartFrom(startFrame);
            var currentXAxisLastKeyFrameValue =
                animation.AnimationCurves[CameraAnimationProperty.AxisX].keys.Last().value;

            Assert.IsTrue(Math.Abs(currentXAxisLastKeyFrameValue - expectedLastFrameXAxis) < 0.0001f);
        }

        [Test]
        public void StartTemplateWithNotSetDOFCurve_ShouldInheritDOFFromStartFrame()
        {
            var animation = CreateTestTemplate(CIRCULAR_ANIM_TEMPLATE_TXT);
            for (var dof = 1f; dof < 100; dof+=0.25f)
            {
                var startFrame = GetRandomStartFrame(dof);
                animation.StartFrom(startFrame);

                var updatedValue = animation.GetValueAtTime(CameraAnimationProperty.DepthOfField, 0);
                Assert.IsTrue(Mathf.Abs(dof - updatedValue) <= 0.00001f);
            }
        }

        private CameraAnimationFrame GetRandomStartFrame(float targetDof)
        {
            var newStartFrameValues = new Dictionary<CameraAnimationProperty, float>
            {
                {CameraAnimationProperty.AxisX, Random.Range(0,180f)},
                {CameraAnimationProperty.AxisY, Random.Range(0,180f)},
                {CameraAnimationProperty.OrbitRadius, Random.Range(10,50f)},
                {CameraAnimationProperty.HeightRadius, Random.Range(10,50f)},
                {CameraAnimationProperty.FieldOfView, Random.Range(10,50f)},
                {CameraAnimationProperty.Dutch, 0},
                {CameraAnimationProperty.DepthOfField, targetDof},
                {CameraAnimationProperty.FocusDistance, 0}
            };
            return CreateFrame(newStartFrameValues);
        }
        
        private CameraAnimationFrame CreateFrame(Dictionary<CameraAnimationProperty, float> startFrameValues)
        {
            return new CameraAnimationFrame(startFrameValues);
        }

        private EditableClip CreateTestTemplate(string animText)
        {
            var clipCurves = CameraAnimationConverter.ConvertToClipData(animText);
            return new CircularAnimation(new[] {CameraAnimationProperty.AxisX}, clipCurves);
        }
    }
}
