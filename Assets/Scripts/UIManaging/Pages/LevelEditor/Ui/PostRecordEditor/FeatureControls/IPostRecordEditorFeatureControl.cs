using Bridge.Models.ClientServer.EditorsSetting;
using Modules.EditorsCommon;
using Modules.EditorsCommon.PostRecordEditor;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.FeatureControls
{
    internal interface IPostRecordEditorFeatureControl: IFeatureControl<PostRecordEditorFeatureType>
    {
    }
    
    internal abstract class PostRecordEditorFeatureControlBase<T> : FeatureControlBase<T, PostRecordEditorFeatureType>, IPostRecordEditorFeatureControl
        where T : class, IPostRecordEditorSetting
    {
    }
}
