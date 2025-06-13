using Bridge.Models.ClientServer.EditorsSetting.Settings;
using JetBrains.Annotations;
using Models;
using Modules.EditorsCommon.LevelEditor;

namespace UIManaging.Pages.LevelEditor.Ui.FeatureControls
{
    internal interface IPreviewLastEventFeatureControl: ILevelEditorFeatureControl
    {
        bool CanPreviewEvent(Event ev);
    }
    
    [UsedImplicitly]
    internal sealed class PreviewLastEventFeatureControl: LevelEditorFeatureControlBase<PreviewLastEventSettings>, IPreviewLastEventFeatureControl
    {
        public override bool IsFeatureEnabled => Settings.AllowPreview;
        public override LevelEditorFeatureType FeatureType => LevelEditorFeatureType.LastEventPreview;

        public bool CanPreviewEvent(Event ev)
        {
            return Settings.AllowPreview;
        }
    }
}