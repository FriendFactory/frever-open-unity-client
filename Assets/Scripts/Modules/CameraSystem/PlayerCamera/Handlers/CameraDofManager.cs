using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Modules.CameraSystem.PlayerCamera.Handlers
{
    [UsedImplicitly]
    internal sealed class CameraDofManager
    {
        private const string VOLUME_PROFILE_PATH = "PostProcessingProfiles/DoFProfile";
        
        private readonly Volume _dofSettings;
        private readonly DepthOfField _dofProfile;

        public CameraDofManager()
        {
            _dofSettings = new GameObject("CameraDepthOfFieldVolume").AddComponent<Volume>();
            var profileVolume = Resources.Load<VolumeProfile>(VOLUME_PROFILE_PATH);
            _dofSettings.profile = profileVolume;
            profileVolume.TryGet(out _dofProfile);
            _dofSettings.enabled = false;
        }
        
        public void SetDepthOfField(float value)
        {
            _dofProfile.focalLength.value = value;
        }

        public void SetFocusDistance(float value)
        {
            _dofProfile.focusDistance.value = value;
        }
        
        public float GetDepthOfField()
        {
            return _dofProfile.focalLength.value;
        }

        public void SwitchDoF(bool isOn)
        {
            _dofSettings.enabled = isOn;
        }
    }
}