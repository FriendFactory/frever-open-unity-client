using Bridge.Models.ClientServer.EditorsSetting.Settings;
using JetBrains.Annotations;
using Modules.EditorsCommon.LevelEditor;

namespace UIManaging.Pages.LevelEditor.Ui.FeatureControls
{
    internal interface ISetLocationSelectionFeatureControl: ILevelEditorFeatureControl
    {
        bool AllowPhoto { get; }
        bool AllowVideo { get; }
    }

    [UsedImplicitly]
    internal sealed class SetLocationSelectionFeatureControl : LevelEditorFeatureControlBase<SetLocationSettings>, ISetLocationSelectionFeatureControl
    {
        public override bool IsFeatureEnabled => Settings.AllowChangeSetLocation && Settings.AllowChangeDayTime &&
                                                 Settings.AllowVideoUploading && Settings.AllowVideoUploading;
        public override LevelEditorFeatureType FeatureType => LevelEditorFeatureType.SetLocationSelection;
        public bool AllowPhoto => Settings.AllowPhotoUploading;
        public bool AllowVideo => Settings.AllowVideoUploading;
    }
}