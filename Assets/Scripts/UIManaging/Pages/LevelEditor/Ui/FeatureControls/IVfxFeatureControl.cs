using Bridge.Models.ClientServer.EditorsSetting.Settings;
using JetBrains.Annotations;
using Modules.EditorsCommon;
using Modules.EditorsCommon.LevelEditor;

namespace UIManaging.Pages.LevelEditor.Ui.FeatureControls
{
    internal interface IVfxFeatureControl : IFeatureControl<LevelEditorFeatureType>
    {
    }

    [UsedImplicitly]
    internal sealed class VfxFeatureControl : LevelEditorFeatureControlBase<VfxSettings>, IVfxFeatureControl
    {
        public override bool IsFeatureEnabled => Settings.AllowEditing;
        public override LevelEditorFeatureType FeatureType => LevelEditorFeatureType.VfxSelection;
    }
}