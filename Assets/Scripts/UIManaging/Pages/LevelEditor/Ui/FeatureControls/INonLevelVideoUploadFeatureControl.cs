using Bridge.Models.ClientServer.EditorsSetting.Settings;
using JetBrains.Annotations;
using Modules.EditorsCommon.LevelEditor;

namespace UIManaging.Pages.LevelEditor.Ui.FeatureControls
{
    internal interface INonLevelVideoUploadFeatureControl: ILevelEditorFeatureControl
    {
    }

    [UsedImplicitly]
    internal sealed class NonLevelVideoUploadFeatureControl : LevelEditorFeatureControlBase<NonLevelVideoUploadSettings>,
                                                              INonLevelVideoUploadFeatureControl
    {
        public override bool IsFeatureEnabled => Settings.AllowUploading;
        public override LevelEditorFeatureType FeatureType => LevelEditorFeatureType.NonLevelVideoUploading;
    }
}