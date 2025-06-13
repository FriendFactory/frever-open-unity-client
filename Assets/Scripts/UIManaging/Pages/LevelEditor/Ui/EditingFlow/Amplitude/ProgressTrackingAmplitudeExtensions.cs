using System.Collections.Generic;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.Amplitude
{
    internal static class ProgressTrackingAmplitudeExtensions
    {
        private static Dictionary<AssetSelectionProgressStepType, string> ASSET_SELECTION_BUTTON_NAMES = new()
        {
            {AssetSelectionProgressStepType.SetLocation, "set_location_button"},
            {AssetSelectionProgressStepType.Vfx, "vfx_button"},
            {AssetSelectionProgressStepType.Sound, "sound_button"},
            {AssetSelectionProgressStepType.Character, "character_button"},
            {AssetSelectionProgressStepType.CameraAnimation, "camera_animation_button"},
            {AssetSelectionProgressStepType.BodyAnimation, "body_animation_button"},
        };
        
        private static Dictionary<WardrobeSelectionProgressStepType, string> WARDROBE_SELECTION_BUTTON_NAMES = new()
        {
            {WardrobeSelectionProgressStepType.Hair, "hair_button"},
            {WardrobeSelectionProgressStepType.Clothes, "clothes_button"},
            {WardrobeSelectionProgressStepType.MakeUp, "makeup_button"},
        };
        
        public static string GetAssetSelectionButtonName(this AssetSelectionProgressStepType stepType) => ASSET_SELECTION_BUTTON_NAMES[stepType];
        public static string GetAssetSelectionButtonClickedEventName(this AssetSelectionProgressStepType stepType) => $"{ASSET_SELECTION_BUTTON_NAMES[stepType]}_clicked";
        
        public static string GetWardrobeSelectionButtonName(this WardrobeSelectionProgressStepType stepType) => WARDROBE_SELECTION_BUTTON_NAMES[stepType];
        public static string GetWardrobeSelectionButtonClickedEventName(this WardrobeSelectionProgressStepType stepType) => $"{WARDROBE_SELECTION_BUTTON_NAMES[stepType]}_clicked";
    }
}