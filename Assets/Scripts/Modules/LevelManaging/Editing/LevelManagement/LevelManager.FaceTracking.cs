using System;
using System.Linq;
using Common;
using JetBrains.Annotations;
using Modules.FaceAndVoice.Face.Facade;
using UnityEngine.XR.ARFoundation;

namespace Modules.LevelManaging.Editing.LevelManagement
{
    internal partial class LevelManager
    {
        private readonly ARFaceManager _arFaceManager;
        private readonly IArSessionManager _arSessionManager;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action UseSameFaceFxChanged;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public bool UseSameFaceFx
        {
            get => _eventEditor.UseSameFaceFx;
            set
            {
                _eventEditor.UseSameFaceFx = value;
                UpdateFaceManagerSettings();
            }
        }

        public bool IsFaceTrackingEnabled { get; private set; }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void SetFaceTracking(bool isEnabled)
        {
            IsFaceTrackingEnabled = isEnabled;
            
            _arSessionManager.SetARActive(isEnabled);
            
            UpdateFaceManagerSettings();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        partial void InitializeFaceTracker()
        {
            CharactersPositionsSwapped += RefreshFaceTrackingTarget;
        }

        private void UpdateFaceManagerSettings()
        {
            if(TargetEvent == null) return;

            if (IsFaceTrackingEnabled)
            {
                UpdateFaceTrackingTargetMesh();
            }
            else
            {
                DeactivateFaceTrackingOnAllCharacters();
            }
        }

        private void UpdateFaceTrackingTargetMesh()
        {
            if (DeviceInformationHelper.DeviceUsesARFoundations())
            {
                _arFaceManager.facesChanged -= ChangeFacePrefab;
                _arFaceManager.facesChanged += ChangeFacePrefab;
            }
            else
            {
                SetupFaceTracking(null);
            }
            
            SetupFaceRecorder();
        }

        private void OnUseSameFaceFxChanged()
        {
            UseSameFaceFxChanged?.Invoke();
        }

        private void RefreshFaceTrackingTarget()
        {
            if (!IsFaceRecordingEnabled) return;
            UpdateFaceManagerSettings();
        }
        
        [UsedImplicitly]
        private void ChangeFacePrefab(ARFacesChangedEventArgs obj)
        {
            //If on iOS we need to wait for ARKit to detect the users face
            //before adding the reference to the recorder.
            var face = obj.updated.FirstOrDefault();
            if (face == null) return;
            face.destroyOnRemoval = false;
            
            SetupFaceTracking(face);
            _arFaceManager.facesChanged -= ChangeFacePrefab;
        }
        
        private void SetupFaceTracking(ARFace arFace)
        {
            if(TargetEvent == null) return;
            RefreshFaceTrackingOnCharacters(arFace);
        }

        private void SetupFaceRecorder()
        {
            var targetCharacter = GetTargetCharacterOrFirstLoaded();
            if (targetCharacter == null) return;
            _faceAnimRecorder.Init(targetCharacter.SkinnedMeshRenderer);
        }
    }
}