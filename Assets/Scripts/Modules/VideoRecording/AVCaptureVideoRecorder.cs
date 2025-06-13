// #define AVPRO_MEASURE_CAPTURING_TIME

using Common;
using Extensions;
using RenderHeads.Media.AVProMovieCapture;
using UnityEngine;

#if AVPRO_MEASURE_CAPTURING_TIME
using System.Diagnostics;
using Debug = UnityEngine.Debug;
#endif

namespace Modules.VideoRecording
{
    [RequireComponent(typeof(CaptureAudioFromAudioRendererCustom))]
    public sealed class AVCaptureVideoRecorder : VideoRecorderBase
    {
        [SerializeField] private CaptureFromTexture _videoCapture;
        [SerializeField] private CaptureAudioFromAudioRendererCustom _audioCapture;

        private FileWritingHandler _writingHandler;
        private LevelViewPort.LevelViewPort _currentViewPort;

        #if AVPRO_MEASURE_CAPTURING_TIME
        private readonly Stopwatch _stopwatch = new();
        #endif

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override string FilePath => VideoCapture ? VideoCapture.LastFilePath : string.Empty;
        public override int AudioDelayFrames
        {
            get => AudioCapture.AudioDelayFrames;
            set => AudioCapture.AudioDelayFrames = value;
        }

        private CaptureFromTexture VideoCapture =>
            _videoCapture ? _videoCapture : _videoCapture = GetComponent<CaptureFromTexture>();
        private CaptureAudioFromAudioRendererCustom AudioCapture =>
            _audioCapture ? _audioCapture : _audioCapture = GetComponent<CaptureAudioFromAudioRendererCustom>();

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Start()
        {
            VideoCapture.BeginFinalFileWritingAction += OnFileWriting;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            VideoCapture.BeginFinalFileWritingAction -= OnFileWriting;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        protected override void SetupRecorder()
        {
            _currentViewPort = FindLevelViewPort();
            ReleaseCurrentRenderTexture(_currentViewPort);

            _currentViewPort.Init(Settings.Resolution);
            EnableViewPortWithFrameDelay();

            VideoCapture.SetSourceTexture(_currentViewPort.RenderTexture);
            VideoCapture.CameraRenderCustomResolution = Settings.Resolution;
            VideoCapture.GetEncoderHints().videoHints.averageBitrate = (uint) Settings.Bitrate;
            VideoCapture.GetEncoderHints().videoHints.maximumBitrate = (uint) Settings.Bitrate;
            VideoCapture.CameraRenderAntiAliasing = Settings.MssaSamples;
        }

        protected override void StartRecording()
        {
            #if AVPRO_MEASURE_CAPTURING_TIME
                _stopwatch.Restart();
                Debug.Log($@"#AVPro# Starting recording...");
            #endif

            VideoCapture.StartCapture();
            EnableViewPortWithFrameDelay();
        }

        protected override void PauseRecording()
        {
            #if AVPRO_MEASURE_CAPTURING_TIME
                _stopwatch.Stop();
                Debug.Log($@"#AVPro# Recording paused: {_stopwatch.Elapsed:mm\:ss\.ff}");
            #endif

            VideoCapture.PauseCapture();
            _currentViewPort.SetActive(false);
        }

        protected override void ResumeRecording()
        {
            #if AVPRO_MEASURE_CAPTURING_TIME
                _stopwatch.Start();
                Debug.Log($@"#AVPro# Recording resumed...");
            #endif

            VideoCapture.ResumeCapture();
            EnableViewPortWithFrameDelay();
        }

        protected override void StopRecording()
        {
            #if AVPRO_MEASURE_CAPTURING_TIME
                _stopwatch.Stop();
                Debug.Log($@"#AVPro# Recording done: {_stopwatch.Elapsed:mm\:ss\.ff}");
            #endif

            VideoCapture.StopCapture();
            _currentViewPort.SetActive(false);
            ReleaseCurrentRenderTexture(_currentViewPort);
        }

        protected override void CancelRecording()
        {
            #if AVPRO_MEASURE_CAPTURING_TIME
                _stopwatch.Stop();
                Debug.Log($@"#AVPro# Recording canceled: {_stopwatch.Elapsed:mm\:ss\.ff}");
            #endif

            VideoCapture.CancelCapture();
            _currentViewPort.SetActive(false);
            ReleaseCurrentRenderTexture(_currentViewPort);
        }

        protected override bool IsFileWritingFinished()
        {
            return _writingHandler != null && _writingHandler.IsFileReady();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        // TODO: Quick workaround to find the viewport and render texture. Would be good to have better solution.
        private static LevelViewPort.LevelViewPort FindLevelViewPort()
        {
            var viewPort = FindObjectOfType(typeof(LevelViewPort.LevelViewPort), true);
            return (LevelViewPort.LevelViewPort) viewPort;
        }

        private static void ReleaseCurrentRenderTexture(LevelViewPort.LevelViewPort viewPort)
        {
            var renderTexture = viewPort?.RenderTexture;
            if (renderTexture == null) return;

            renderTexture.Release();
            Destroy(renderTexture);
        }

        private void OnFileWriting(FileWritingHandler handler)
        {
            //Set the handler and start checking if the file is ready to copy.
            _writingHandler = handler;
        }

        // Workaround for mirror shaders issue as they're using previous frame data
        private void EnableViewPortWithFrameDelay()
        {
            CoroutineSource.Instance.ExecuteWithFrameDelay(() => _currentViewPort.SetActive(true));
        }
    }
}