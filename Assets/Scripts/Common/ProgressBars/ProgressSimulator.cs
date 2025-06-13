using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Common.ProgressBars
{
    public sealed class ProgressSimulator : MonoBehaviour
    {
        [SerializeField] private bool _resetOnEnable = true;
        [SerializeField] private ProgressSimulatorSettings _settings;

        private IProgressBar _progress;

        private RootCanvasClosingTracker _rootClosingTracker;

        private float _prevFrameRealTime;
        private float _progressBarDuration;
        private float _currentBarValue;
        private AnimationCurve _currentProgressCurve;
        private float _speed;
        private float _elapsedTime;

        public Action<float> ProgressUpdated;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void StartSimulation()
        {
            enabled = true;
        }

        public void StopSimulation()
        {
            enabled = false;
        }
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _progress = GetComponent<IProgressBar>();

            var rootTransparencyControl = GetComponentInParent<CanvasGroup>();
            if (rootTransparencyControl != null)
            {
                _rootClosingTracker = new RootCanvasClosingTracker(rootTransparencyControl);
                _rootClosingTracker.ParentClosingDetected += CompleteProgressBarQuickly;
            }
        }

        private void OnEnable()
        {
            _prevFrameRealTime = Time.realtimeSinceStartup;
            _progressBarDuration = Random.Range(_settings.MinDuration, _settings.MaxDuration);
            _currentProgressCurve = _settings.Curves[Random.Range(0, _settings.Curves.Count)];

            _currentBarValue = 0f;
            if (_resetOnEnable) SetProgressValue(0f);

            _speed = 1;
            _elapsedTime = 0;
            _rootClosingTracker?.Run();
        }

        private void Update()
        {
            _rootClosingTracker?.Update();

            var deltaTime = Time.realtimeSinceStartup - _prevFrameRealTime;
            _elapsedTime += deltaTime * _speed;
            var nextValue = _currentProgressCurve.Evaluate(_elapsedTime / _progressBarDuration);
            nextValue = Mathf.Clamp(nextValue, _currentBarValue, 0.99f); //never reach end
            SetProgressValue(nextValue);
            _prevFrameRealTime = Time.realtimeSinceStartup;
        }

        private void OnDisable()
        {
            _rootClosingTracker?.Stop();
        }

        private void OnValidate()
        {
            if (_progress == null)
            {
                _progress = GetComponentInChildren<IProgressBar>();
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void CompleteProgressBarQuickly()
        {
            _speed = 15;
        }

        private void SetProgressValue(float value)
        {
            _currentBarValue = value;
            if (_progress == null)
            {
                ProgressUpdated?.Invoke(value);
                return;
            }
            _progress.Value = _currentBarValue;
        }

        private class RootCanvasClosingTracker
        {
            public event Action ParentClosingDetected;

            private readonly CanvasGroup _canvasGroup;
            private float _previousTransparencyValue;
            private bool _isRunning;

            public RootCanvasClosingTracker(CanvasGroup canvasGroup)
            {
                _canvasGroup = canvasGroup;
            }

            public void Run()
            {
                _previousTransparencyValue = _canvasGroup.alpha;
                _isRunning = true;
            }

            public void Update()
            {
                if (!_isRunning) return;

                var currentTransparencyValue = _canvasGroup.alpha;
                if (_previousTransparencyValue > currentTransparencyValue)
                {
                    ParentClosingDetected?.Invoke();
                }

                _previousTransparencyValue = currentTransparencyValue;
            }

            public void Stop()
            {
                _isRunning = false;
            }
        }
    }
}