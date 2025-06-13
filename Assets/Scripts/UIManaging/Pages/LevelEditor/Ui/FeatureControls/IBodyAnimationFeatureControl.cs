using Bridge.Models.ClientServer.EditorsSetting.Settings;
using JetBrains.Annotations;
using Modules.EditorsCommon.LevelEditor;

namespace UIManaging.Pages.LevelEditor.Ui.FeatureControls
{
    internal interface IBodyAnimationFeatureControl : ILevelEditorFeatureControl
    {
    }

    [UsedImplicitly]
    internal sealed class BodyAnimationFeatureControl : LevelEditorFeatureControlBase<BodyAnimationSettings>, IBodyAnimationFeatureControl
    {
        public override LevelEditorFeatureType FeatureType => LevelEditorFeatureType.BodyAnimationSelection;
        public override bool IsFeatureEnabled => Settings.AllowEditing;
    }
}