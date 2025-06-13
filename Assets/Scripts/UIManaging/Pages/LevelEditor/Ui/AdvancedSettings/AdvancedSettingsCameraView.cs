using System.Linq;
using JetBrains.Annotations;
using Modules.CameraSystem.CameraSystemCore;
using Navigation.Core;
using UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabs.AdvancedCameraTabs;
using UIManaging.Pages.LevelEditor.Ui.AssetSettingsViews;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.AdvancedSettings
{
    internal sealed class AdvancedSettingsCameraView : MultiTabAdvancedSettingsView
    {
        [SerializeField] private Button _changeCameraAngleButton;
        [SerializeField] private GameObject _editCameraPositionPanel;
        
        private ICameraSystem _cameraSystem;
        private AdvancedCameraTab[] _cameraTabs;

        private PageManager _pageManager;
        private BaseEditorPageModel _editorPageModel;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        [UsedImplicitly]
        public void Construct(PageManager pageManager, ICameraSystem cameraSystem, BaseEditorPageModel pageModel)
        {
            _pageManager = pageManager;
            _cameraSystem = cameraSystem;
            _editorPageModel = pageModel;
            _cameraTabs = Tabs.Cast<AdvancedCameraTab>().ToArray();
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            if (IsPostRecordEditorPage())
            {
                _changeCameraAngleButton.gameObject.SetActive(true);
                _changeCameraAngleButton.onClick.AddListener(OnClickChangeCameraAngleButton);
            }

            foreach (var tab in _cameraTabs)
            {
                tab.SettingChanged += OnSettingChanged;
            }
        }

        private void OnEnable()
        {
            _cameraSystem.CameraSettingsChanged += RefreshSettings;

            if (!IsPostRecordEditorPage()) _cameraSystem.StopAnimation();
        }

        private void OnDisable()
        {
            _cameraSystem.CameraSettingsChanged -= RefreshSettings;
        }

        private void OnDestroy()
        {
            foreach (var tab in _cameraTabs)
            {
                tab.SettingChanged -= OnSettingChanged;
            }

            _changeCameraAngleButton.onClick.RemoveListener(OnClickChangeCameraAngleButton);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void RefreshSettings()
        {
            foreach (var tab in _cameraTabs)
            {
                if (!IsSetup)
                {
                    tab.Reset();
                    continue;
                }

                if (tab.ResetOnSettingsChanged)
                {
                    tab.Reset();
                }
            }
        }

        private void OnClickChangeCameraAngleButton()
        {
            _editorPageModel.OnChangeCameraAngleClicked();
            _editorPageModel.PostRecordEditorUnsubscribeFromEvents();
            _editorPageModel.RemovePostRecordEditorCameraTexture();

            _editCameraPositionPanel.SetActive(true);
        }

        private bool IsPostRecordEditorPage()
        {
            return _pageManager.IsCurrentPage(PageId.PostRecordEditor);
        }
    }
}