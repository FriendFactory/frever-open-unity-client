using Bridge.Models.ClientServer.EditorsSetting.Settings;
using JetBrains.Annotations;
using Modules.EditorsCommon.LevelEditor;

namespace UIManaging.Pages.LevelEditor.Ui.FeatureControls
{
    internal interface ICharacterSelectionFeatureControl: ILevelEditorFeatureControl
    {
    }

    [UsedImplicitly]
    internal sealed class CharacterSelectionFeatureControl :
        LevelEditorFeatureControlBase<CharacterSettings>, ICharacterSelectionFeatureControl
    {
        public override bool IsFeatureEnabled => Settings.AllowEditing;
        public override LevelEditorFeatureType FeatureType => LevelEditorFeatureType.CharacterSelection;
    }
}