using JetBrains.Annotations;
using Modules.VideoRecording;
using NatSuite.Devices;
using NatSuite.Devices.Internal;

namespace Modules.AudioOutputManaging.iOS
{
    [UsedImplicitly]
    internal sealed class DeviceAudioOutputControl: IDeviceAudioOutputControl
    {
        private readonly IVideoRecorder _videoRecorder;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public DeviceAudioOutputControl(IVideoRecorder videoRecorder)
        {
            _videoRecorder = videoRecorder;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Run()
        {
            SetupAudioSession();

            //for some reason on iOS the video recording changes the audio settings (audio comes from top speaker). So we have to re-configure audio settings after video recording is finished.
            _videoRecorder.RecordingEnded += ConfigureAudioSettings;
            _videoRecorder.RecordingCanceled += ConfigureAudioSettings;
        }

        public void Stop()
        {
            _videoRecorder.RecordingEnded -= ConfigureAudioSettings;
            _videoRecorder.RecordingCanceled -= ConfigureAudioSettings;
        }

        private void SetupAudioSession()
        {
            #if UNITY_IOS
            var query = new MediaDeviceQuery(MediaDeviceCriteria.AudioDevice);
            if (query.count > 1) query.Advance();
            #endif
        }

        private static void ConfigureAudioSettings()
        {
            NatDeviceExt.ConfigureAudioSession();
        }
    }
}