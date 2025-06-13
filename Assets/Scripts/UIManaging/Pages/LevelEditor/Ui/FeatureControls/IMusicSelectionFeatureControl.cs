using Bridge.Models.ClientServer.EditorsSetting.Settings;
using JetBrains.Annotations;
using Modules.EditorsCommon.LevelEditor;

namespace UIManaging.Pages.LevelEditor.Ui.FeatureControls
{
    internal interface IMusicSelectionFeatureControl: ILevelEditorFeatureControl
    {
        bool AllowUserSoundSelection { get; }
    }

    [UsedImplicitly]
    internal sealed class MusicSelectionFeatureControl : LevelEditorFeatureControlBase<MusicSettings>, IMusicSelectionFeatureControl
    {
        public override bool IsFeatureEnabled => Settings.AllowEditing;
        public override LevelEditorFeatureType FeatureType => LevelEditorFeatureType.MusicSelection;
        public bool AllowUserSoundSelection => Settings.UserSoundSettings.AllowUserSounds;
    }
}