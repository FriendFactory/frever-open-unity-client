using Bridge.Models.ClientServer.EditorsSetting;
using Modules.EditorsCommon;
using Modules.EditorsCommon.LevelEditor;

namespace UIManaging.Pages.LevelEditor.Ui.FeatureControls
{
    internal interface ILevelEditorFeatureControl: IFeatureControl<LevelEditorFeatureType>
    {
    }
    
    internal abstract class LevelEditorFeatureControlBase<T> : FeatureControlBase<T, LevelEditorFeatureType>, ILevelEditorFeatureControl
        where T : class, ILevelEditorSetting
    {
    }
}
