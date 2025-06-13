using JetBrains.Annotations;
using Modules.EditorsCommon.LevelEditor;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.LevelEditor.Ui.Common;

namespace UIManaging.Pages.LevelEditor.Ui.FeatureControls
{
    [UsedImplicitly]
    internal sealed class OutfitFeatureControl: OutfitFeatureControlBase<LevelEditorFeatureType>, ILevelEditorFeatureControl
    {
        public OutfitFeatureControl(LocalUserDataHolder localUserDataHolder, ILevelManager levelManager) : base(localUserDataHolder, levelManager)
        {
        }

        public override LevelEditorFeatureType FeatureType => LevelEditorFeatureType.OutfitSelection;
    }
}