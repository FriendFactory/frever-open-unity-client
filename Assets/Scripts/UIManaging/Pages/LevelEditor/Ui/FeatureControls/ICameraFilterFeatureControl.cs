using Bridge.Models.ClientServer.EditorsSetting.Settings;
using JetBrains.Annotations;
using Modules.EditorsCommon.LevelEditor;

namespace UIManaging.Pages.LevelEditor.Ui.FeatureControls
{
    internal interface ICameraFilterFeatureControl : ILevelEditorFeatureControl
    {
    }

    [UsedImplicitly]
    internal sealed class CameraFilterFeatureControl : LevelEditorFeatureControlBase<CameraFilterSettings>, ICameraFilterFeatureControl
    {
        public override bool IsFeatureEnabled => Settings.AllowEditing;
        public override LevelEditorFeatureType FeatureType => LevelEditorFeatureType.CameraFilterSelection;
    }
}