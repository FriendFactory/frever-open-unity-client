using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace Modules.CameraSystem.PlayerCamera.Handlers
{
    internal sealed class CameraNoiseProfileHandler
    {
        private readonly List<CinemachineBasicMultiChannelPerlin> _noiseComponents = new List<CinemachineBasicMultiChannelPerlin>();
        
        public int CurrentNoiseProfileId { get; private set; }
        public NoiseSettings[] NoiseProfiles { get; private set; }
        public event Action<int> NoiseProfileChanged;

        public void Setup(IEnumerable<CinemachineVirtualCamera> rigs)
        {
            var profiles = Resources.LoadAll<NoiseSettings>("Noise/");
            NoiseProfiles = profiles;

            foreach (var rig in rigs)
            {
                _noiseComponents.Add(rig.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>());
            }
        }
        
        public void SetNoiseProfile(int cameraNoiseIndex)
        {
            foreach (var noiseComponent in _noiseComponents)
            {
                noiseComponent.m_NoiseProfile = NoiseProfiles[cameraNoiseIndex];
            }

            CurrentNoiseProfileId = cameraNoiseIndex;
            NoiseProfileChanged?.Invoke(CurrentNoiseProfileId);
        }
    }
}
