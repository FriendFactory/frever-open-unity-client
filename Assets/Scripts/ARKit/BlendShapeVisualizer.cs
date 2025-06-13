using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;


namespace ARKit
{
    [RequireComponent(typeof(ARFace))]
    public abstract class BlendShapeVisualizer : MonoBehaviour
    {
        protected const float CUSTOM_BLENDSHAPE_VALUE = 0;
        protected SkinnedMeshRenderer SkinnedMeshRenderer;
        protected ARFace Face;
        private readonly string[] _customBlendShapes = 
        {
            BlendShapesConstants.CustomBlendShapes.EYE_BLINK_SQUINT_CORR_LEFT,
            BlendShapesConstants.CustomBlendShapes.EYE_BLINK_SQUINT_CORR_RIGHT,
            BlendShapesConstants.CustomBlendShapes.MOUTH_UPPER_SMILE_CORR_LEFT,
            BlendShapesConstants.CustomBlendShapes.MOUTH_UPPER_SMILE_CORR_RIGHT
        };
        
        //Events
        
        public event Action RequestPlayerCenterFaceStarted;
        public event Action RequestPlayerCenterFaceFinished;
        
        public event Action RequestPlayerNeedsBetterLightingStarted;
        public event Action RequestPlayerNeedsBetterLightingFinished;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected virtual void Awake()
        {}
        protected virtual void OnEnable()
        {
            SetVisible(true);
        }
        
        private void LateUpdate()
        {
            UpdateFaceFeatures();
        }
        

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public virtual void Init(SkinnedMeshRenderer faceMesh)
        {
            SkinnedMeshRenderer = faceMesh;
        }
        
        public void SetArFace(ARFace face)
        {
            if (face != null)
            {
                Face = face;
                Face.destroyOnRemoval = false;
            }

            OnEnable();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected int GetBlendShapeIndex(string blendShapeName)
        {
            return SkinnedMeshRenderer.sharedMesh.GetBlendShapeIndex($"{BlendShapesConstants.BLENDSHAPES_PREFIX}.{blendShapeName}");
        }

        protected bool IsBlendShapeMeshValid()
        {
            return SkinnedMeshRenderer != null && SkinnedMeshRenderer.enabled && SkinnedMeshRenderer.sharedMesh != null;
        }

        protected virtual void UpdateFaceFeatures()
        {
            UpdateCustomBlendShapes();
        }

        protected void PlayerNeedsToCenterFaceStart()
        {
            RequestPlayerCenterFaceStarted?.Invoke();
        }

        protected void OnPlayerNeedsToCenterFaceFinished()
        {
            RequestPlayerCenterFaceFinished?.Invoke();
        }   
        
        protected void PlayerNeedsBetterLightingStarted()
        {
            RequestPlayerNeedsBetterLightingStarted?.Invoke();
        }

        protected void OnPlayerNeedsBetterLightingFinished()
        {
            RequestPlayerNeedsBetterLightingFinished?.Invoke();
        } 
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        protected void SetVisible(bool visible)
        {
            if (SkinnedMeshRenderer == null) return;
            SkinnedMeshRenderer.enabled = visible;
        }

        //In some cases body animation will have custom blendshapes for face added to them.
        //We need to override these blendshapes in order to prevent strange artifacts while using face tracking. 
        private void UpdateCustomBlendShapes()
        {
            foreach (var blendShape in _customBlendShapes)
            {
                SkinnedMeshRenderer.SetBlendShapeWeight(GetBlendShapeIndex(blendShape), CUSTOM_BLENDSHAPE_VALUE);
            }
        }

    }
}