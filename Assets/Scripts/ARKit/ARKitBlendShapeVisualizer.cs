#if UNITY_IOS
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARKit;

namespace ARKit
{
    public sealed class ARKitBlendShapeVisualizer : BlendShapeVisualizer
    {
        private const float COEFFICIENT_SCALE = 100.0f;
        private ARKitFaceSubsystem _arKitFaceSubsystem;
        private Dictionary<ARKitBlendShapeLocation, int> _faceArkitBlendShapeIndexMap;

        public override void Init(SkinnedMeshRenderer faceMesh)
        {
            base.Init(faceMesh);
            CreateFeatureBlendMapping();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            var faceManager = FindObjectOfType<ARFaceManager>();
            if (faceManager != null)
            {
                _arKitFaceSubsystem = (ARKitFaceSubsystem) faceManager.subsystem;
            }
            ARSession.stateChanged += OnSystemStateChanged;
        }

        private void OnDisable()
        {
            ARSession.stateChanged -= OnSystemStateChanged;
        }

        private void CreateFeatureBlendMapping()
        {
            if (SkinnedMeshRenderer == null || SkinnedMeshRenderer.sharedMesh == null) return;
            
            _faceArkitBlendShapeIndexMap = new Dictionary<ARKitBlendShapeLocation, int>
            {
                [ARKitBlendShapeLocation.BrowDownLeft] = GetBlendShapeIndex(BlendShapesConstants.BROW_DOWN_LEFT),
                [ARKitBlendShapeLocation.BrowDownRight] = GetBlendShapeIndex(BlendShapesConstants.BROW_DOWN_RIGHT),
                [ARKitBlendShapeLocation.BrowInnerUp] = GetBlendShapeIndex(BlendShapesConstants.BROW_INNER_UP),
                [ARKitBlendShapeLocation.BrowOuterUpLeft] = GetBlendShapeIndex(BlendShapesConstants.BROW_OUTER_UP_LEFT),
                [ARKitBlendShapeLocation.BrowOuterUpRight] = GetBlendShapeIndex(BlendShapesConstants.BROW_OUTER_UP_RIGHT),
                [ARKitBlendShapeLocation.CheekPuff] = GetBlendShapeIndex(BlendShapesConstants.CHEEK_PUFF),
                [ARKitBlendShapeLocation.CheekSquintLeft] = GetBlendShapeIndex(BlendShapesConstants.CHEEK_SQUINT_LEFT),
                [ARKitBlendShapeLocation.CheekSquintRight] = GetBlendShapeIndex(BlendShapesConstants.CHEEK_SQUINT_RIGHT),
                [ARKitBlendShapeLocation.EyeBlinkLeft] = GetBlendShapeIndex(BlendShapesConstants.EYE_BLINK_LEFT),
                [ARKitBlendShapeLocation.EyeBlinkRight] = GetBlendShapeIndex(BlendShapesConstants.EYE_BLINK_RIGHT),
                [ARKitBlendShapeLocation.EyeLookDownLeft] = GetBlendShapeIndex(BlendShapesConstants.EYE_LOOK_DOWN_LEFT),
                [ARKitBlendShapeLocation.EyeLookDownRight] = GetBlendShapeIndex(BlendShapesConstants.EYE_LOOK_DOWN_RIGHT),
                [ARKitBlendShapeLocation.EyeLookInLeft] = GetBlendShapeIndex(BlendShapesConstants.EYE_LOOK_IN_LEFT),
                [ARKitBlendShapeLocation.EyeLookInRight] = GetBlendShapeIndex(BlendShapesConstants.EYE_LOOK_IN_RIGHT),
                [ARKitBlendShapeLocation.EyeLookOutLeft] = GetBlendShapeIndex(BlendShapesConstants.EYE_LOOK_OUT_LEFT),
                [ARKitBlendShapeLocation.EyeLookOutRight] = GetBlendShapeIndex(BlendShapesConstants.EYE_LOOK_OUT_RIGHT),
                [ARKitBlendShapeLocation.EyeLookUpLeft] = GetBlendShapeIndex(BlendShapesConstants.EYE_LOOK_UP_LEFT),
                [ARKitBlendShapeLocation.EyeLookUpRight] = GetBlendShapeIndex(BlendShapesConstants.EYE_LOOK_UP_RIGHT),
                [ARKitBlendShapeLocation.EyeSquintLeft] = GetBlendShapeIndex(BlendShapesConstants.EYE_SQUINT_LEFT),
                [ARKitBlendShapeLocation.EyeSquintRight] = GetBlendShapeIndex(BlendShapesConstants.EYE_SQUINT_RIGHT),
                [ARKitBlendShapeLocation.EyeWideLeft] = GetBlendShapeIndex(BlendShapesConstants.EYE_WIDE_LEFT),
                [ARKitBlendShapeLocation.EyeWideRight] = GetBlendShapeIndex(BlendShapesConstants.EYE_WIDE_RIGHT),
                [ARKitBlendShapeLocation.JawForward] = GetBlendShapeIndex(BlendShapesConstants.JAW_FORWARD),
                [ARKitBlendShapeLocation.JawLeft] = GetBlendShapeIndex(BlendShapesConstants.JAW_LEFT),
                [ARKitBlendShapeLocation.JawOpen] = GetBlendShapeIndex(BlendShapesConstants.JAW_OPEN),
                [ARKitBlendShapeLocation.JawRight] = GetBlendShapeIndex(BlendShapesConstants.JAW_RIGHT),
                [ARKitBlendShapeLocation.MouthClose] = GetBlendShapeIndex(BlendShapesConstants.MOUTH_CLOSE),
                [ARKitBlendShapeLocation.MouthDimpleLeft] = GetBlendShapeIndex(BlendShapesConstants.MOUTH_DIMPLE_LEFT),
                [ARKitBlendShapeLocation.MouthDimpleRight] = GetBlendShapeIndex(BlendShapesConstants.MOUTH_DIMPLE_RIGHT),
                [ARKitBlendShapeLocation.MouthFrownLeft] = GetBlendShapeIndex(BlendShapesConstants.MOUTH_FROWN_LEFT),
                [ARKitBlendShapeLocation.MouthFrownRight] = GetBlendShapeIndex(BlendShapesConstants.MOUTH_FROWN_RIGHT),
                [ARKitBlendShapeLocation.MouthFunnel] = GetBlendShapeIndex(BlendShapesConstants.MOUTH_FUNNEL),
                [ARKitBlendShapeLocation.MouthLeft] = GetBlendShapeIndex(BlendShapesConstants.MOUTH_LEFT),
                [ARKitBlendShapeLocation.MouthLowerDownLeft] = GetBlendShapeIndex(BlendShapesConstants.MOUTH_LOWER_DOWN_LEFT),
                [ARKitBlendShapeLocation.MouthLowerDownRight] = GetBlendShapeIndex(BlendShapesConstants.MOUTH_LOWER_DOWN_RIGHT),
                [ARKitBlendShapeLocation.MouthPressLeft] = GetBlendShapeIndex(BlendShapesConstants.MOUTH_PRESS_LEFT),
                [ARKitBlendShapeLocation.MouthPressRight] = GetBlendShapeIndex(BlendShapesConstants.MOUTH_PRESS_RIGHT),
                [ARKitBlendShapeLocation.MouthPucker] = GetBlendShapeIndex(BlendShapesConstants.MOUTH_PUCKER),
                [ARKitBlendShapeLocation.MouthRight] = GetBlendShapeIndex(BlendShapesConstants.MOUTH_RIGHT),
                [ARKitBlendShapeLocation.MouthRollLower] = GetBlendShapeIndex(BlendShapesConstants.MOUTH_ROLL_LOWER),
                [ARKitBlendShapeLocation.MouthRollUpper] = GetBlendShapeIndex(BlendShapesConstants.MOUTH_ROLL_UPPER),
                [ARKitBlendShapeLocation.MouthShrugLower] = GetBlendShapeIndex(BlendShapesConstants.MOUTH_SHRUG_LOWER),
                [ARKitBlendShapeLocation.MouthShrugUpper] = GetBlendShapeIndex(BlendShapesConstants.MOUTH_SHRUG_UPPER),
                [ARKitBlendShapeLocation.MouthSmileLeft] = GetBlendShapeIndex(BlendShapesConstants.MOUTH_SMILE_LEFT),
                [ARKitBlendShapeLocation.MouthSmileRight] = GetBlendShapeIndex(BlendShapesConstants.MOUTH_SMILE_RIGHT),
                [ARKitBlendShapeLocation.MouthStretchLeft] = GetBlendShapeIndex(BlendShapesConstants.MOUTH_STRETCH_LEFT),
                [ARKitBlendShapeLocation.MouthStretchRight] = GetBlendShapeIndex(BlendShapesConstants.MOUTH_STRETCH_RIGHT),
                [ARKitBlendShapeLocation.MouthUpperUpLeft] = GetBlendShapeIndex(BlendShapesConstants.MOUTH_UPPER_UP_LEFT),
                [ARKitBlendShapeLocation.MouthUpperUpRight] = GetBlendShapeIndex(BlendShapesConstants.MOUTH_UPPER_UP_RIGHT),
                [ARKitBlendShapeLocation.NoseSneerLeft] = GetBlendShapeIndex(BlendShapesConstants.NOSE_SNEER_LEFT),
                [ARKitBlendShapeLocation.NoseSneerRight] = GetBlendShapeIndex(BlendShapesConstants.NOSE_SNEER_RIGHT),
                [ARKitBlendShapeLocation.TongueOut] = GetBlendShapeIndex(BlendShapesConstants.TONGUE_OUT)
            };
        }

        protected override void UpdateFaceFeatures()
        {
            if (!IsBlendShapeMeshValid()) return;
            if (Face == null) return;

            using (var blendShapes = _arKitFaceSubsystem.GetBlendShapeCoefficients(Face.trackableId, Allocator.Temp))
            {
                foreach (var featureCoefficient in blendShapes)
                {
                    if (!_faceArkitBlendShapeIndexMap.TryGetValue(featureCoefficient.blendShapeLocation,
                                                                  out var mappedBlendShapeIndex)) continue;
                    if (mappedBlendShapeIndex < 0) continue;
                    SkinnedMeshRenderer.SetBlendShapeWeight(mappedBlendShapeIndex,
                                                            featureCoefficient.coefficient * COEFFICIENT_SCALE);
                }
            }

            base.UpdateFaceFeatures();
        }
        
        private void OnSystemStateChanged(ARSessionStateChangedEventArgs eventArgs)
        {
            SetVisible(true);
        }
    }
}
#endif