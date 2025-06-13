using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Modules.CameraSystem.CameraAnimations.Template;
using Modules.CameraSystem.CameraAnimations.Template.ScriptableObjects;
using UnityEngine;

namespace Modules.CameraSystem.CameraAnimations
{
    internal sealed class CameraAnimationHolder
    {
        private const string RESOURCES_PATH = "ScriptableObjects/TemplateCameraAnimations/";

        private TemplateCameraAnimationClip[] _templateCameraAnimationClips;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public TemplateCameraAnimationClip[] GetAllTemplateCameraAnimationClips()
        {
            return _templateCameraAnimationClips;
        }

        public void LoadAllLocalTemplateAnimationClips()
        {
            _templateCameraAnimationClips = new TemplateCameraAnimationClip[]
            {
                new SteadyAnimation(new[]
                    {
                        CameraAnimationProperty.AxisX,
                    },
                    LoadTemplateCurves("Still"))
                {
                    SpeedIsAdjustable = false,
                    IsLoopable = true,
                },
                new CircularAnimation(new[]
                {
                    CameraAnimationProperty.AxisX,
                }, LoadTemplateCurves("Circular Right"))
                {
                    IsLoopable = true,
                    IsEndlessPlayable = true,
                },
                new PanVerticallyAnimation(new[]
                {
                    CameraAnimationProperty.AxisY,
                }, LoadTemplateCurves("Pan Up")),
                new ZoomAnimation(new[]
                {
                    CameraAnimationProperty.OrbitRadius,
                    CameraAnimationProperty.HeightRadius,
                }, LoadTemplateCurves("Zoom In")),
                new SpiralAnimation(new[]
                {
                    CameraAnimationProperty.AxisX,
                    CameraAnimationProperty.OrbitRadius,
                    CameraAnimationProperty.HeightRadius,
                }, LoadTemplateCurves( "Spiral")),
                new DownwardSpiralAnimation(new[]
                {
                    CameraAnimationProperty.AxisY,
                    CameraAnimationProperty.AxisX,
                    CameraAnimationProperty.OrbitRadius,
                    CameraAnimationProperty.HeightRadius
                }, LoadTemplateCurves("Downward Spiral")),
                new PanVerticallyAnimation(new[]
                {
                    CameraAnimationProperty.AxisY,
                }, LoadTemplateCurves("Pan Up")) {IsReversible = true},
                new ZoomAnimation(new[]
                {
                    CameraAnimationProperty.OrbitRadius,
                    CameraAnimationProperty.HeightRadius,
                }, LoadTemplateCurves("Zoom In")) {IsReversible = true},
                new SpiralAnimation(new[]
                {
                    CameraAnimationProperty.AxisX,
                    CameraAnimationProperty.OrbitRadius,
                    CameraAnimationProperty.HeightRadius,
                }, LoadTemplateCurves("Spiral")) {IsReversible = true},
                new DownwardSpiralAnimation(new[]
                {
                    CameraAnimationProperty.AxisY,
                    CameraAnimationProperty.AxisX,
                    CameraAnimationProperty.OrbitRadius,
                    CameraAnimationProperty.HeightRadius,
                }, LoadTemplateCurves("Downward Spiral")) {IsReversible = true},
                new ZoomOutAnimation(new[]
                {
                    CameraAnimationProperty.OrbitRadius,
                    CameraAnimationProperty.HeightRadius,
                }, LoadTemplateCurves("Zoom Out")),
                new ZoomOutAnimation(new[]
                    {
                        CameraAnimationProperty.OrbitRadius,
                        CameraAnimationProperty.HeightRadius,
                    }, LoadTemplateCurves("Zoom Out")) {IsReversible = true},
                new ZoomOutSpinSpiralAnimation(new[]
                {
                    CameraAnimationProperty.OrbitRadius,
                    CameraAnimationProperty.HeightRadius,
                    CameraAnimationProperty.Dutch,
                    CameraAnimationProperty.AxisX,
                }, LoadTemplateCurves("Zoom Out Spin Spiral")),
                new ZoomOutSpinSpiralAnimation(new[]
                    {
                        CameraAnimationProperty.OrbitRadius,
                        CameraAnimationProperty.HeightRadius,
                        CameraAnimationProperty.Dutch,
                        CameraAnimationProperty.AxisX,
                    }, LoadTemplateCurves("Zoom Out Spin Spiral")) {IsReversible = true},
                new PanDownSpinSpiralAnimation(new[]
                {
                    CameraAnimationProperty.Dutch,
                    CameraAnimationProperty.AxisX,
                    CameraAnimationProperty.AxisY,
                    CameraAnimationProperty.OrbitRadius,
                    CameraAnimationProperty.HeightRadius,
                }, LoadTemplateCurves("Pan Down Spin Spiral")),
                new PanDownSpinSpiralAnimation(new[]
                {
                    CameraAnimationProperty.Dutch,
                    CameraAnimationProperty.AxisX,
                    CameraAnimationProperty.AxisY,
                    CameraAnimationProperty.OrbitRadius,
                    CameraAnimationProperty.HeightRadius,
                }, LoadTemplateCurves("Pan Down Spin Spiral")) {IsReversible = true},
                new ZoomInSpinAnimation(new[]
                {
                    CameraAnimationProperty.OrbitRadius,
                    CameraAnimationProperty.HeightRadius,
                    CameraAnimationProperty.Dutch
                }, LoadTemplateCurves("Zoom In Spin")),
                new ZoomInSpinAnimation(new[]
                {
                    CameraAnimationProperty.OrbitRadius,
                    CameraAnimationProperty.HeightRadius,
                    CameraAnimationProperty.Dutch
                }, LoadTemplateCurves("Zoom In Spin")) {IsReversible = true},
                new CircularAnimation(new[]
                {
                    CameraAnimationProperty.AxisX,
                }, LoadTemplateCurves("Circular Left"))
                {
                    IsLoopable = true,
                    IsEndlessPlayable = true,
                },
                new PanVerticallyAnimation(new[]
                {
                    CameraAnimationProperty.AxisY,
                }, LoadTemplateCurves("Pan Down")),
                new SpinAnimation(new[]
                {
                    CameraAnimationProperty.Dutch,
                }, LoadTemplateCurves("Spin Right")),
                new SpinAnimation(new[]
                {
                    CameraAnimationProperty.Dutch,
                }, LoadTemplateCurves("Spin Left")),
                new SlideSpin(new[]
                {
                    CameraAnimationProperty.Dutch,
                    CameraAnimationProperty.AxisX
                }, LoadTemplateCurves("Circular Left Spin Left")),
                new SlideSpin(new[]
                {
                    CameraAnimationProperty.Dutch,
                    CameraAnimationProperty.AxisX
                }, LoadTemplateCurves("Circular Left Spin Right")),
                new SlideSpin(new[]
                {
                    CameraAnimationProperty.Dutch,
                    CameraAnimationProperty.AxisX
                }, LoadTemplateCurves("Circular Right Spin Right")),
                new SlideSpin(new[]
                {
                    CameraAnimationProperty.Dutch,
                    CameraAnimationProperty.AxisX
                }, LoadTemplateCurves("Circular Right Spin Left")),
                new ZoomAnimation(new[]
                {
                    CameraAnimationProperty.OrbitRadius,
                    CameraAnimationProperty.HeightRadius,
                    CameraAnimationProperty.FieldOfView,
                }, LoadTemplateCurves("Zoom In FishEye")),
                new SlideSpin(new[]
                {
                    CameraAnimationProperty.Dutch,
                    CameraAnimationProperty.AxisX,
                    CameraAnimationProperty.AxisY
                }, LoadTemplateCurves("Circular Left Pan Up Spin Right")),
                new SlideSpin(new[]
                {
                    CameraAnimationProperty.Dutch,
                    CameraAnimationProperty.AxisX,
                    CameraAnimationProperty.AxisY
                }, LoadTemplateCurves("Circular Right Pan Up Spin Left")),
                new SlideSpin(new[]
                {
                    CameraAnimationProperty.Dutch,
                    CameraAnimationProperty.AxisX,
                    CameraAnimationProperty.AxisY
                }, LoadTemplateCurves("Circular Left Pan Down Spin Left")),
                new SlideSpin(new[]
                {
                    CameraAnimationProperty.Dutch,
                    CameraAnimationProperty.AxisX,
                    CameraAnimationProperty.AxisY
                }, LoadTemplateCurves("Circular Right Pan Down Spin Right")),
                new ZoomOutAnimation(new[]
                {
                    CameraAnimationProperty.OrbitRadius,
                    CameraAnimationProperty.HeightRadius,
                    CameraAnimationProperty.Dutch
                }, LoadTemplateCurves("Zoom Out Fast Spin Right")),
                new ZoomOutAnimation(new[]
                {
                    CameraAnimationProperty.OrbitRadius,
                    CameraAnimationProperty.HeightRadius,
                    CameraAnimationProperty.Dutch
                }, LoadTemplateCurves("Zoom Out Fast Spin Left")),
                new ZoomAnimation(new[]
                {
                    CameraAnimationProperty.OrbitRadius,
                    CameraAnimationProperty.HeightRadius,
                    CameraAnimationProperty.AxisY,
                }, LoadTemplateCurves("Zoom In Pan Down")),
                new ZoomAnimation(new[]
                {
                    CameraAnimationProperty.OrbitRadius,
                    CameraAnimationProperty.HeightRadius,
                    CameraAnimationProperty.AxisX,
                }, LoadTemplateCurves("Zoom In Circular Right")),
                new ZoomAnimation(new[]
                {
                    CameraAnimationProperty.OrbitRadius,
                    CameraAnimationProperty.HeightRadius,
                    CameraAnimationProperty.AxisX,
                }, LoadTemplateCurves("Zoom In Circular Left")),
                new ZoomOutAnimation(new[]
                {
                    CameraAnimationProperty.OrbitRadius,
                    CameraAnimationProperty.HeightRadius,
                    CameraAnimationProperty.AxisY,
                }, LoadTemplateCurves("Zoom Out Pan Up")),
            };
        }

        private IDictionary<CameraAnimationProperty, AnimationCurve> LoadTemplateCurves(string fileName)
        {
            var templateData = Resources.Load<CameraAnimationTemplateData>($"{RESOURCES_PATH}{fileName}");
            return templateData.AnimationCurves;
        }

        public void SetIdsForTemplates(CameraAnimationTemplate[] models)
        {
            models = models.OrderBy(x => x.Id).ToArray();

            for (var i = 0; i < models.Length; i++)
            {
                if (i >= _templateCameraAnimationClips.Length)
                    break;
                _templateCameraAnimationClips[i].Id = models[i].Id;
            }
        }
    }
}