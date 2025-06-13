using System;
using UnityEngine;

namespace ARKit
{
    public abstract class NonTrueDepthBlendShapeVisualizer : BlendShapeVisualizer
    {
        protected void SetBlendShapes(float[] blendShapeValues)
        {
            if (GetBlendShapeIndex(BlendShapesConstants.JAW_OPEN) < 0) return;
            if (blendShapeValues.Length != BlendShapesConstants.BLENDSHAPE_COUNT)
            {
                throw new InvalidOperationException($"Blendshapes count need to match amount: {BlendShapesConstants.BLENDSHAPE_COUNT}");
            }
            
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.JAW_OPEN), blendShapeValues[NonDepthBlendShapeConstants.JAW_OPEN]);
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.JAW_FORWARD), blendShapeValues[NonDepthBlendShapeConstants.JAW_FORWARD] );
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.JAW_LEFT), blendShapeValues[NonDepthBlendShapeConstants.JAW_LEFT] );
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.JAW_RIGHT), blendShapeValues[NonDepthBlendShapeConstants.JAW_RIGHT] );
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.NOSE_SNEER_LEFT), blendShapeValues[NonDepthBlendShapeConstants.NOSE_SNEER_LEFT] );
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.NOSE_SNEER_RIGHT), blendShapeValues[NonDepthBlendShapeConstants.NOSE_SNEER_RIGHT] );
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.CHEEK_SQUINT_LEFT), blendShapeValues[NonDepthBlendShapeConstants.CHEEK_SQUINT_LEFT] );
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.CHEEK_SQUINT_RIGHT), blendShapeValues[NonDepthBlendShapeConstants.CHEEK_SQUINT_RIGHT] );
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.BROW_INNER_UP), blendShapeValues[NonDepthBlendShapeConstants.BROW_UP] );
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.BROW_OUTER_UP_LEFT), blendShapeValues[NonDepthBlendShapeConstants.BROW_UP] );
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.BROW_OUTER_UP_RIGHT), blendShapeValues[NonDepthBlendShapeConstants.BROW_UP] );
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.BROW_DOWN_LEFT), blendShapeValues[NonDepthBlendShapeConstants.BROW_DOWN] );
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.BROW_DOWN_RIGHT), blendShapeValues[NonDepthBlendShapeConstants.BROW_DOWN] );
            
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.EYE_BLINK_LEFT), blendShapeValues[NonDepthBlendShapeConstants.EYE_BLINK_LEFT]);
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.EYE_BLINK_RIGHT), blendShapeValues[NonDepthBlendShapeConstants.EYE_BLINK_RIGHT]);
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.EYE_WIDE_LEFT), blendShapeValues[NonDepthBlendShapeConstants.EYE_WIDE_LEFT]);
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.EYE_WIDE_RIGHT), blendShapeValues[NonDepthBlendShapeConstants.EYE_WIDE_RIGHT]);
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.EYE_SQUINT_LEFT), blendShapeValues[NonDepthBlendShapeConstants.EYE_SQUINT_LEFT]);
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.EYE_SQUINT_RIGHT), blendShapeValues[NonDepthBlendShapeConstants.EYE_SQUINT_RIGHT]);

            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.MOUTH_SMILE_LEFT), blendShapeValues[NonDepthBlendShapeConstants.MOUTH_SMILE_LEFT]);
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.MOUTH_SMILE_RIGHT), blendShapeValues[NonDepthBlendShapeConstants.MOUTH_SMILE_RIGHT]);
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.MOUTH_CLOSE), blendShapeValues[NonDepthBlendShapeConstants.MOUTH_CLOSE]);
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.MOUTH_FUNNEL), blendShapeValues[NonDepthBlendShapeConstants.MOUTH_FUNNEL]);
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.MOUTH_PUCKER), blendShapeValues[NonDepthBlendShapeConstants.MOUTH_PUCKER]);
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.MOUTH_ROLL_UPPER), blendShapeValues[NonDepthBlendShapeConstants.MOUTH_ROLL_UPPER]);
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.MOUTH_ROLL_LOWER), blendShapeValues[NonDepthBlendShapeConstants.MOUTH_ROLL_LOWER]);
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.MOUTH_LEFT), blendShapeValues[NonDepthBlendShapeConstants.MOUTH_LEFT]);
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.MOUTH_RIGHT), blendShapeValues[NonDepthBlendShapeConstants.MOUTH_RIGHT]);
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.MOUTH_DIMPLE_LEFT), blendShapeValues[NonDepthBlendShapeConstants.MOUTH_DIMPLE_LEFT]);
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.MOUTH_DIMPLE_RIGHT), blendShapeValues[NonDepthBlendShapeConstants.MOUTH_DIMPLE_RIGHT]);
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.MOUTH_STRETCH_LEFT), blendShapeValues[NonDepthBlendShapeConstants.MOUTH_STRETCH_LEFT]);
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.MOUTH_STRETCH_RIGHT), blendShapeValues[NonDepthBlendShapeConstants.MOUTH_STRETCH_RIGHT]);
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.MOUTH_UPPER_UP_LEFT), blendShapeValues[NonDepthBlendShapeConstants.MOUTH_UPPER_UP_LEFT]);
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.MOUTH_UPPER_UP_RIGHT), blendShapeValues[NonDepthBlendShapeConstants.MOUTH_UPPER_UP_RIGHT]);
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.MOUTH_LOWER_DOWN_LEFT), blendShapeValues[NonDepthBlendShapeConstants.MOUTH_LOWER_DOWN_LEFT]);
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.MOUTH_LOWER_DOWN_RIGHT), blendShapeValues[NonDepthBlendShapeConstants.MOUTH_LOWER_DOWN_RIGHT]);
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.MOUTH_FROWN_LEFT), blendShapeValues[NonDepthBlendShapeConstants.MOUTH_FROWN_LEFT]);
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.MOUTH_FROWN_RIGHT), blendShapeValues[NonDepthBlendShapeConstants.MOUTH_FROWN_RIGHT]);
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.MOUTH_SHRUG_LOWER), blendShapeValues[NonDepthBlendShapeConstants.MOUTH_SHRUG_LOWER]);
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.MOUTH_SHRUG_UPPER), blendShapeValues[NonDepthBlendShapeConstants.MOUTH_SHRUG_UPPER]);
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.MOUTH_PRESS_LEFT), blendShapeValues[NonDepthBlendShapeConstants.MOUTH_PRESS_LEFT]);
            SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(BlendShapesConstants.MOUTH_PRESS_RIGHT), blendShapeValues[NonDepthBlendShapeConstants.MOUTH_PRESS_RIGHT]);
        }
    }
}
