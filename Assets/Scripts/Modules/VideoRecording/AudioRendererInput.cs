/* 
*   NatCorder
*   Copyright (c) 2021 Yusuf Olokoba.
*/

using System.Collections.Generic;
using Unity.Collections;

namespace NatSuite.Recorders.Inputs
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Recorder input for recording audio frames directly from AudioRenderer.
    /// It uses AVProCapture audio rendering approach and manual control of frame committing, so,
    /// has almost nothing in common with NatCorder inputs.
    /// </summary>
    public sealed class AudioRendererInput : IDisposable
    {
    #region --Client API--

        public int AudioDelayFrames { get; set; }

        public AudioRendererInput()
        {
            _unityAudioChannelCount = GetUnityAudioChannelCount();

            // Start Capturing
            StartCapture();
        }

        public void Dispose()
        {
            StopCapture();
        }

    #endregion
        
        public float[] GetSamples()
        {
            var sampleFrameCount = AudioRenderer.GetSampleCountForCaptureFrame();
            var sampleCount = sampleFrameCount * _unityAudioChannelCount;

            // We needed a way to delay recorded audio for some amount of frames to be perfectly synchronized
            // with ARKit face capture, so added this workaround
            if (_framesCaptured == 0)
            {
                var silence = new float[sampleCount];
                for (var i = 0; i < AudioDelayFrames; i++)
                {
                    _samplesQueue.Enqueue(silence);
                }
            }

            NativeArray<float> audioSamples = new NativeArray<float>(sampleCount, Allocator.TempJob);
            if (AudioRenderer.Render(audioSamples))
            {
                // TODO: use NativeArray instead of converting to array for less GC (but not super important in offline mode)
                _samplesQueue.Enqueue(audioSamples.ToArray());
            }

            audioSamples.Dispose();

            _framesCaptured++;

            return _samplesQueue.Dequeue();
        }

    #region --Operations--

        private readonly int _unityAudioChannelCount;

        private bool _isRendererRecording;

        private readonly Queue<float[]> _samplesQueue = new Queue<float[]>();
        private int _framesCaptured;

        private void StartCapture()
        {
            if (!_isRendererRecording)
            {
                AudioRenderer.Start();
                _isRendererRecording = true;

                _samplesQueue.Clear();
                _framesCaptured = 0;
            }

            FlushBuffer();
        }

        private void StopCapture()
        {
            if (_isRendererRecording)
            {
                _isRendererRecording = false;
                AudioRenderer.Stop();
            }
        }

        private void FlushBuffer()
        {
            int sampleFrameCount = AudioRenderer.GetSampleCountForCaptureFrame();
            int sampleCount = sampleFrameCount * _unityAudioChannelCount;
            NativeArray<float> audioSamples = new NativeArray<float>(sampleCount, Allocator.Temp);
            AudioRenderer.Render(audioSamples);
            audioSamples.Dispose();
        }

        // FROM AVPRO
        public static int GetUnityAudioChannelCount()
        {
            int result = GetChannelCount(AudioSettings.driverCapabilities);
            if (
                #if !UNITY_2019_2_OR_NEWER
				AudioSettings.speakerMode != AudioSpeakerMode.Raw &&
                #endif
                AudioSettings.speakerMode < AudioSettings.driverCapabilities)
            {
                result = GetChannelCount(AudioSettings.speakerMode);
            }

            return result;
        }

        private static int GetChannelCount(AudioSpeakerMode mode)
        {
            int result = 0;
            switch (mode)
            {
                #if !UNITY_2019_2_OR_NEWER
				case AudioSpeakerMode.Raw:
					break;
                #endif
                case AudioSpeakerMode.Mono:
                    result = 1;
                    break;
                case AudioSpeakerMode.Stereo:
                    result = 2;
                    break;
                case AudioSpeakerMode.Quad:
                    result = 4;
                    break;
                case AudioSpeakerMode.Surround:
                    result = 5;
                    break;
                case AudioSpeakerMode.Mode5point1:
                    result = 6;
                    break;
                case AudioSpeakerMode.Mode7point1:
                    result = 8;
                    break;
                case AudioSpeakerMode.Prologic:
                    result = 2;
                    break;
            }

            return result;
        }

    #endregion
    }
}