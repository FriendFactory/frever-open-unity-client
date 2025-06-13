using Extensions;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.AssetSettingsViews
{
    public class CameraFilterSettingsView : AssetSettingsView
    {
        [SerializeField] private CameraFilterValueAssetView cameraFilterValueAssetView;

        private ILevelManager _levelManager;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        [UsedImplicitly]
        private void Construct(ILevelManager levelManager)
        {
            _levelManager = levelManager;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _levelManager.AssetUpdateCompleted += OnAssetUpdated;
        }

        private void OnDestroy()
        {
            _levelManager.AssetUpdateCompleted -= OnAssetUpdated;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override void PartialDisplay()
        {
            UpdateFilterValueSilently();
            cameraFilterValueAssetView.Display(true);
        }

        public override void PartialHide()
        {
            cameraFilterValueAssetView.Display(false);
        }

        public override void Setup()
        {
            cameraFilterValueAssetView.Setup();
            UpdateFilterValueSilently();
            cameraFilterValueAssetView.ValueChanged += OnValueChanged;
        }

        private void OnValueChanged(float value)
        {
            _levelManager.SetFilterValue(value);
        }

        private void UpdateFilterValue()
        {
            OnValueChanged(cameraFilterValueAssetView.CurrentValue);
            UpdateFilterValueSilently();
        }

        private void OnAssetUpdated(DbModelType modelType)
        {
            if (modelType != DbModelType.CameraFilterVariant) return;
            UpdateFilterValue();
        }

        private void UpdateFilterValueSilently()
        {
            var filterController = _levelManager.TargetEvent.GetCameraFilterController();
            if (filterController?.CameraFilterValue == null) return;
            
            var value = filterController.CameraFilterValue.Value.ToHecto();
            cameraFilterValueAssetView.SetValueSilent(value);
        }
    }
}