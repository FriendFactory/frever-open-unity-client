using System;
using System.Linq;
using JetBrains.Annotations;
using NatSuite.Devices;
using NatSuite.Devices.Outputs;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Modules.FaceAndVoice.Voice.Recording.Core
{
    [UsedImplicitly]
    internal sealed class VoiceRecorder : IVoiceRecorder
    {
        private AudioDevice _microphone;
        private VoiceClip _voiceClip;
        private AudioClipOutput _clipOutput;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public AudioClip AudioClip => _voiceClip?.AudioClip;
        public bool IsRecording { get; set; }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public VoiceRecorder()
        {
            Reset();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void StartRecording()
        {
            if (_microphone != null && _microphone.running)
            {
                return;
            }
            
#if !UNITY_ANDROID || UNITY_EDITOR 
            SetupMicrophone();
#endif
            StartMicrophone();
        }

        public VoiceClip StopRecording()
        {
            IsRecording = false;

            StopRunningMic();
            _voiceClip.AudioClip = _clipOutput.ToClip();
            return _voiceClip;
        }

        public void CancelRecording()
        {
            StopRunningMic();

            if (_voiceClip != null)
            {
                if (_voiceClip.AudioClip != null)
                {
                    _voiceClip.AudioClip.UnloadAudioData();
                }

                Object.Destroy(_voiceClip.AudioClip);
                _voiceClip.AudioClip = null;
            }

            Reset();
        }

        public void SetMicrophone(string microphoneId)
        {
            MediaDeviceQuery.ConfigureAudioSession = false;
            var query = new MediaDeviceQuery(MediaDeviceCriteria.AudioDevice);
            _microphone = query.FirstOrDefault(mic => mic.uniqueID == microphoneId) as AudioDevice;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        [UsedImplicitly]
        private void SetupMicrophone()
        {
            MediaDeviceQuery.ConfigureAudioSession = false;
            var query = new MediaDeviceQuery(MediaDeviceCriteria.AudioDevice);

            var microphoneIndex = 0;

            #if UNITY_IOS
            if (query.count > 1) microphoneIndex++;
            #endif

            _microphone = query[microphoneIndex] as AudioDevice;
        }

        private void Reset()
        {
            _voiceClip = new VoiceClip();
            IsRecording = false;
        }

        private void StopRunningMic()
        {
            if (_microphone == null || !_microphone.running) return;
            _microphone.StopRunning();
        }
        
        private void StartMicrophone()
        {
            if (_microphone == null)
            {
                return;
            }
            
            _clipOutput = new AudioClipOutput();
            _microphone.StartRunning(_clipOutput);
            IsRecording = true;
        }
    }
}
