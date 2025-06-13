using System;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.AssetSettingsViews
{
    public class CameraAnimationSettingsView : AdvancedAssetSettingsView
    {
        [SerializeField] private CameraAnimationSpeedAssetView cameraAnimationSpeedAssetView;
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override void Setup()
        {
            cameraAnimationSpeedAssetView.Setup();
            AddListenerToAdvancedSettingsButton(PartialHide);
            _advancedSettingsView.Hidden -= PartialDisplay;
            _advancedSettingsView.Hidden += PartialDisplay;
            base.Setup();
        }

        public override void PartialHide()
        {
            cameraAnimationSpeedAssetView.ForceHide = true;
            cameraAnimationSpeedAssetView.Display(false);
        }

        public override void PartialDisplay()
        {
            var shouldDisplay = cameraAnimationSpeedAssetView.IsClipSpeedAdjustable;
            cameraAnimationSpeedAssetView.ForceHide = false;
            cameraAnimationSpeedAssetView.Display(shouldDisplay);
        }

        public void ResetAllSettings()
        {
            cameraAnimationSpeedAssetView.Reset();
            ResetAdvancedView();
        }
    }
}
