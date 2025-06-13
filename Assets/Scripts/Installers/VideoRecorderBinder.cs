using Modules.VideoRecording;
using UnityEngine;
using Zenject;

namespace Installers
{
    internal static class VideoRecorderBinder
    {
        public static void BindVideoRecorder(this DiContainer container, VideoRecorderBase videoCapturePrefab)
        {
            container.Bind<IVideoRecorder>()
                     .To<AVCaptureVideoRecorder>()
                     .FromComponentInNewPrefab(videoCapturePrefab)
                     .AsSingle().NonLazy();
        }
    }
}