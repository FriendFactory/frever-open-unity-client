using Bridge.Models.ClientServer.EditorsSetting.Settings;
using JetBrains.Annotations;
using Modules.EditorsCommon.LevelEditor;

namespace UIManaging.Pages.LevelEditor.Ui.FeatureControls
{
    internal interface ITemplateFeatureControl : ILevelEditorFeatureControl
    {
    }

    [UsedImplicitly]
    internal sealed class TemplateFeatureControl : LevelEditorFeatureControlBase<TemplateSettings>,
                                                   ITemplateFeatureControl
    {
        public override bool IsFeatureEnabled => Settings.AllowEditing;
        public override LevelEditorFeatureType FeatureType => LevelEditorFeatureType.TemplateSelection;
    }
}