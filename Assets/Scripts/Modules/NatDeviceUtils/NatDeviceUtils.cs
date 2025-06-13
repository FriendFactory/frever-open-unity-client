using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NatSuite.Devices;
using NatSuite.Devices.Outputs;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NatDeviceUtils
{
    public static class NatDeviceUtils
    {
        private const float TEST_RECORDING_TIME = 1f;
        
        public static IEnumerator QueryMicrophones(Action<string> onCompleted)
        {
            var query = new MediaDeviceQuery(device => device is AudioDevice);

            foreach (var device in query)
            {
                var audioDevice = device as AudioDevice;
                var audioClipOutput = new AudioClipOutput();
                Debug.Log($"Querying AudioDevice [{device.uniqueID}] {device.name} {device.location.ToString()} {device.defaultForMediaType} {device}");
                audioDevice.StartRunning(audioClipOutput);
                yield return new WaitForSeconds(TEST_RECORDING_TIME);
                audioDevice.StopRunning();
                var recordedSomething = audioClipOutput.HasRecording;
                audioClipOutput.Dispose();
                
                if (recordedSomething)
                {
                    onCompleted?.Invoke(audioDevice.uniqueID);
                    yield break;
                }
 
                yield return null;
            }
            
            Debug.Log($"MicrophoneCalibration could not record with neither available device");
            
            onCompleted?.Invoke(null);
        }
    }
}