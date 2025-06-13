using Bridge.Models.ClientServer.EditorsSetting.Settings;
using Extensions;
using JetBrains.Annotations;
using Modules.EditorsCommon.LevelEditor;

namespace UIManaging.Pages.LevelEditor.Ui.FeatureControls
{
    internal interface ICameraAnimationFeatureControl : ILevelEditorFeatureControl
    {
        bool AllowToUseAllTemplates { get; }
        long[] TemplateIds { get; }
    }

    [UsedImplicitly]
    internal sealed class CameraAnimationFeatureControl : LevelEditorFeatureControlBase<CameraAnimationSettings>,
                                                          ICameraAnimationFeatureControl
    {
        public override bool IsFeatureEnabled => Settings.AllowEditing;
        public override LevelEditorFeatureType FeatureType => LevelEditorFeatureType.CameraAnimationSelection;
        public bool AllowToUseAllTemplates => Settings.AllTemplatesAvailable;
        public long[] TemplateIds => Settings.TemplateIds;
    }
}