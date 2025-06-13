using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Common;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.AssetSelectors;

namespace UIManaging.Pages.LevelEditor.Ui.AssetUIManagers
{
    internal abstract class BaseCharactersUIHandler : BaseAssetUIHandler
    {
        protected static readonly ICategory[] CATEGORIES = 
        {
            new CharacterInfoCategory { Id = Constants.MY_FREVERS_TAB_INDEX },
            new CharacterInfoCategory { Id = Constants.FRIENDS_TAB_INDEX },
            new CharacterInfoCategory { Id = Constants.STAR_CREATORS_TAB_INDEX },
        };
        
        protected readonly BaseEditorPageModel PageModel;
        protected readonly ILevelManager LevelManager;
        protected bool SuppressCharacterViewUpdate;

        public abstract CharacterUIHandlerType Type { get;}

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        protected BaseCharactersUIHandler(BaseEditorPageModel pageModel, ILevelManager levelManager)
        {
            PageModel = pageModel;
            LevelManager = levelManager;
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public abstract void SetupView(CharacterFullInfo[] selectedItems, long universeId);

        public abstract void Initialize();

        public abstract void CleanUp();
    }
}
