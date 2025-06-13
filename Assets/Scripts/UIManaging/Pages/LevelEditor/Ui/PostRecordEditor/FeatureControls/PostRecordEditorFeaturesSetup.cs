using Bridge.Models.ClientServer.EditorsSetting;
using JetBrains.Annotations;
using Modules.EditorsCommon;
using Modules.EditorsCommon.PostRecordEditor;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.FeatureControls
{
    [UsedImplicitly]
    internal sealed class PostRecordEditorFeaturesSetup: FeaturesSetup<IPostRecordEditorFeatureControl, IPostRecordEditorSetting, PostRecordEditorSettings, PostRecordEditorFeatureType>
    {
        public PostRecordEditorFeaturesSetup(IPostRecordEditorFeatureControl[] featureControls) : base(featureControls)
        {
        }
    }
}