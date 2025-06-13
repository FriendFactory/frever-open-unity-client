using System;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.RenderingPipelineManagement;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui
{
    public class EditorRenderingPerformanceManager : MonoBehaviour
    {
        public float FPSTarget = 30;
        public float SettleTimeIncrease = 10f;
        public float SettleTimeDecrease = 0.1f;
        public float RenderScaleMin = 0.3f;
        public float RenderScaleMax = 0.6f;
        public float IncreaseScaleIncrement = 0.05f;
        public float DecreaseScaleIncrement = 0.1f;
        
        [Inject] private ILevelManager _levelManager;
        [Inject] private IRenderingPipelineManager _renderingPipelineManager;
        [SerializeField] private AssetSelectionViewManager assetSelectionViewManager;
        private short _sampleIndex;
        private short _sampleCount;
        private short _sampleCapcity = 60;
        private short[] _fpsArray;
        private short _currentfps;
        private float _averageFPS;
        private float _nextAdjustTimeDecrease;
        private float _nextAdjustTimeIncrease;
        private float _previousLowFramerate;
        private bool _isDebuggerEnabled = false;
        private float _initialRenderScale;
        private bool _paused = true;

        private void Awake()
        {
            _fpsArray = new short[_sampleCapcity];
            _previousLowFramerate = FPSTarget;
        }
        
        private void OnEnable()
        {
        #if UNITY_ANDROID
            var urpAsset = (UniversalRenderPipelineAsset) GraphicsSettings.renderPipelineAsset;
            _initialRenderScale = urpAsset.renderScale;
            _levelManager.EventLoadingCompleted += OnEditorLoadingCompleted;
            if (assetSelectionViewManager)
            {
                assetSelectionViewManager.Opened += OnAssetSelectionViewManagerOpened;
                assetSelectionViewManager.Closed += OnAssetSelectionViewManagerClosed;
            }
        #endif
        }

        private void OnDisable()
        {
        #if UNITY_ANDROID
            _levelManager.EventLoadingCompleted -= OnEditorLoadingCompleted;
            if (!_renderingPipelineManager.IsHighQuality())
            {
                var urpAsset = (UniversalRenderPipelineAsset) GraphicsSettings.renderPipelineAsset;
                urpAsset.renderScale = _initialRenderScale;
            }

            if (assetSelectionViewManager)
            {
                assetSelectionViewManager.Opened -= OnAssetSelectionViewManagerOpened;
                assetSelectionViewManager.Closed -= OnAssetSelectionViewManagerClosed;
            }
        #endif
        }

        private void OnEditorLoadingCompleted()
        {
            _paused = false;
        }
        
        private void OnAssetSelectionViewManagerClosed()
        {
            _paused = false;
        }
        
        private void OnAssetSelectionViewManagerOpened()
        {
            _paused = true;
        }

        private void LateUpdate()
        {
        #if UNITY_ANDROID
            if (_paused || _renderingPipelineManager.IsHighQuality()) // we don't want to mess with offline rendering
                return;
            
            SampleData();
            AdjustRenderScale();
        #endif
        }

        private void SampleData()
        {
            _currentfps = (short) (Mathf.RoundToInt(1f / Time.unscaledDeltaTime));
            _sampleIndex++;
            if (_sampleIndex >= _sampleCapcity)
                _sampleIndex = 0;

            _fpsArray[_sampleIndex] = _currentfps;
            if (_sampleCount < _sampleCapcity)
            {
                _sampleCount++;
            }

            uint sum = 0;
            for (int i = 0; i < _sampleCount; i++)
            {
                sum += (uint) _fpsArray[i];
            }

            _averageFPS = (short) (sum / (float) _sampleCount);
        }

        private void AdjustRenderScale()
        {
            if (_averageFPS <
                FPSTarget) // may wish for a window here so that we are not oscillating, but the _nextAdjustTime and average provide some of this.
            {
                if (Time.time > _nextAdjustTimeDecrease)
                {
                    _previousLowFramerate = _averageFPS;
                    DecreaseRenderScale();
                    _nextAdjustTimeDecrease = Time.time + SettleTimeDecrease;
                }
            }
            else
            {
                if (Time.time > _nextAdjustTimeIncrease)
                {
                    IncreaseRenderScale();
                    _nextAdjustTimeIncrease = Time.time + SettleTimeIncrease;
                }
            }
        }

        void DecreaseRenderScale()
        {
            var urpAsset = (UniversalRenderPipelineAsset) GraphicsSettings.renderPipelineAsset;
            urpAsset.renderScale -= DecreaseScaleIncrement;
            if (urpAsset.renderScale < RenderScaleMin)
                urpAsset.renderScale = RenderScaleMin;
        }

        void IncreaseRenderScale()
        {
            var urpAsset = (UniversalRenderPipelineAsset) GraphicsSettings.renderPipelineAsset;
            urpAsset.renderScale += IncreaseScaleIncrement;
            if (urpAsset.renderScale > RenderScaleMax)
                urpAsset.renderScale = RenderScaleMax;
        }

        private void OnGUI()
        {
            if (!_isDebuggerEnabled)
                return;

            int midFont = (int) ((Screen.height / 1334f * 3) * 8 * 0.7f);
            GUI.skin.label.fontSize = midFont;
            GUI.skin.button.fontSize = midFont;
            GUI.skin.textField.fontSize = midFont;

            int width = (int) (Screen.width * 0.20f);
            int y = 500;
            int height = (int) (Screen.height * 0.04f);
            int nextSpot = height + (int) (Screen.height * 0.005f);
            var urpAsset = (UniversalRenderPipelineAsset) GraphicsSettings.renderPipelineAsset;

            string debug = String.Format(
                " render scale:{0} fps average:{1} fps last low:{2} paused: {3}",
                urpAsset.renderScale,
                _averageFPS,
                _previousLowFramerate,
                _paused
            );
            GUI.Label(new Rect(10, y, Screen.width * 0.9f, Screen.height * 0.4f), debug);

            y += nextSpot;

            debug = String.Format(
                "scale min: {0} scale max: {1} \nTime2Decrease: {2} Time2Increase {3}",
                RenderScaleMin,
                RenderScaleMax,
                SettleTimeDecrease,
                SettleTimeIncrease
            );
            GUI.Label(new Rect(10, y, Screen.width * 0.9f, Screen.height * 0.4f), debug);


            y += (int) (Screen.height * 0.15);

            if (GUI.Button(new Rect(10, y, width, height), "rscale+"))
            {
                IncreaseRenderScale();
            }

            y += nextSpot;
            if (GUI.Button(new Rect(10, y, width, height), "rscale-"))
            {
                DecreaseRenderScale();
            }

            y += nextSpot;
            if (GUI.Button(new Rect(10, y, width, height), "rscaleMax+"))
            {
                RenderScaleMax += 0.05f;
            }

            y += nextSpot;
            if (GUI.Button(new Rect(10, y, width, height), "rscaleMax-"))
            {
                RenderScaleMax -= 0.05f;
            }

            y += nextSpot;
            if (GUI.Button(new Rect(10, y, width, height), "rscaleMin+"))
            {
                RenderScaleMin += 0.05f;
            }

            y += nextSpot;
            if (GUI.Button(new Rect(10, y, width, height), "rscaleMin-"))
            {
                RenderScaleMin -= 0.05f;
            }

            y += nextSpot;
            if (GUI.Button(new Rect(10, y, width, height), "Time2Increase+"))
            {
                SettleTimeIncrease += 0.1f;
            }

            y += nextSpot;
            if (GUI.Button(new Rect(10, y, width, height), "Time2Increase-"))
            {
                SettleTimeIncrease -= 0.1f;
            }

            y += nextSpot;
            if (GUI.Button(new Rect(10, y, width, height), "Time2Decrease+"))
            {
                SettleTimeDecrease += 0.1f;
            }

            y += nextSpot;
            if (GUI.Button(new Rect(10, y, width, height), "Time2Decrease-"))
            {
                SettleTimeDecrease -= 0.1f;
            }

        }
    }
}
