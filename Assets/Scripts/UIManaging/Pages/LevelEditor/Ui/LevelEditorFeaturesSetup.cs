using Bridge.Models.ClientServer.EditorsSetting;
using JetBrains.Annotations;
using Modules.EditorsCommon;
using Modules.EditorsCommon.LevelEditor;
using UIManaging.Pages.LevelEditor.Ui.FeatureControls;

namespace UIManaging.Pages.LevelEditor.Ui
{
    [UsedImplicitly]
    internal sealed class LevelEditorFeaturesSetup: FeaturesSetup<ILevelEditorFeatureControl, ILevelEditorSetting, LevelEditorSettings, LevelEditorFeatureType>
    {
        public LevelEditorFeaturesSetup(ILevelEditorFeatureControl[] featureControls) : base(featureControls)
        {
        }
    }
}