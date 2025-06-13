using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace EditorTools
{
    /// <summary>
    /// Checks the scriptable object for broken and missed references
    /// It runs on Postprocess, not on Preprocess, because of having issue on batchmode build
    /// </summary>
    internal sealed class CheckPopupPresetsBeforeBuild: IPostprocessBuildWithReport
    {
        private const string PRESETS_ASSETS_PATH = "Assets/Configs/PopupPresetsConfig.asset";
        private const string PRESETS_FIELD_NAME = "_popupPresets";
        
        public int callbackOrder { get; }
        
        public void OnPostprocessBuild(BuildReport report)
        {
            var popupPresets = AssetDatabase.LoadAssetAtPath<PopupPresetsConfig>(PRESETS_ASSETS_PATH);
            if (popupPresets == null)
                throw new BuildFailedException($"Can't find popup presets asset at path: {PRESETS_ASSETS_PATH}");

            ValidateIfNoNullReferences(popupPresets);
            ValidateIfItContainsAllPopupTypes(popupPresets);
        }

        private void ValidateIfNoNullReferences(PopupPresetsConfig config)
        {
            var targetListProperty =
                typeof(PopupPresetsConfig).GetField(PRESETS_FIELD_NAME, BindingFlags.Instance | BindingFlags.NonPublic);
            
            if (targetListProperty == null)
                throw new BuildFailedException($"Can't find field {PRESETS_FIELD_NAME} in {nameof(PopupPresetsConfig)}. Please fix validation");
            
            if (!(targetListProperty.GetValue(config) is ICollection<BasePopup> presetList))
                throw new BuildFailedException($"Popup preset list is null or changed type");
            
            if (presetList.Any(x=>x == null))
                throw new BuildFailedException($"Popup preset list contains broken links (null references). Broken references count: {presetList.Count(x=>x == null)}");
        }

        private void ValidateIfItContainsAllPopupTypes(PopupPresetsConfig config)
        {
            foreach (PopupType popupType in Enum.GetValues(typeof(PopupType)))
            {
                var popupPreset = config.GetPresetByType(popupType);
                if (popupPreset == null)
                {
                    throw new BuildFailedException($"PopupType {popupType} does not have a prefab link in presets list. Please add {popupType} prefab to {PRESETS_ASSETS_PATH}");
                }
            }
        }
    }
}