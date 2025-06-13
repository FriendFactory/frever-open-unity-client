using Bridge.Models.ClientServer.EditorsSetting.Settings;
using Extensions;
using JetBrains.Annotations;
using Modules.EditorsCommon.PostRecordEditor;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.FeatureControls
{
    internal interface ICameraAnimationFeatureControl: IPostRecordEditorFeatureControl
    {
        long[] TemplateIds { get; }
        bool AllowToUseAllTemplates { get; }
    }

    [UsedImplicitly]
    internal sealed class CameraAnimationFeatureControl : PostRecordEditorFeatureControlBase<CameraAnimationSettings>, ICameraAnimationFeatureControl
    {
        public override bool IsFeatureEnabled => Settings.AllowEditing;
        public override PostRecordEditorFeatureType FeatureType => PostRecordEditorFeatureType.CameraSelection;
        public long[] TemplateIds => Settings.TemplateIds;
        public bool AllowToUseAllTemplates => Settings.AllTemplatesAvailable;
    }
}