using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.EditorsSetting;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.LevelEditor.Ui;
using UIManaging.Pages.LevelEditor.Ui.Common;

namespace UIManaging.Pages.ProfilePhotoEditor.Pages
{
    /// <summary>
    /// Hotfix for FREV-10224. We have to resolve all dependencies for character selection even though we don't use this feature here
    /// todo: Long term solution is described in ticket FREV-10230
    /// </summary>
    [UsedImplicitly]
    internal sealed class ProfilePhotoEditorPageModel: BaseEditorPageModel
    {
        public ProfilePhotoEditorPageModel(ILevelManager levelManager) : base(levelManager, null) //passing 'null' to follow keeping working current short term solution; should be refactored with the entire page  
        {
        }

        public override void OnShoppingCartOpened()
        {
        }

        public override void OnShoppingCartClosed()
        {
        }

        public override bool CanChangeOutfitForTargetCharacter(ref string message) => false;

        public override bool CanChangeOutfitForTargetCharacter() => false;

        public override bool CanCreateNewOutfitForTargetCharacter() => false;
    }
}