using Bridge.Models.ClientServer.EditorsSetting.Settings;
using JetBrains.Annotations;
using Modules.EditorsCommon.LevelEditor;

namespace UIManaging.Pages.LevelEditor.Ui.FeatureControls
{
    internal interface IVoiceFilterFeatureControl: ILevelEditorFeatureControl
    {
        bool AllowDisablingVoiceFilter { get; }
    }

    [UsedImplicitly]
    internal sealed class VoiceFilterFeatureControl : LevelEditorFeatureControlBase<VoiceFilterSettings>,
                                                      IVoiceFilterFeatureControl
    {
        public override bool IsFeatureEnabled => Settings.AllowEditing;
        public override LevelEditorFeatureType FeatureType => LevelEditorFeatureType.VoiceFilterSelection;
        public bool AllowDisablingVoiceFilter => Settings.AllowDisablingVoiceFilter;
    }
}