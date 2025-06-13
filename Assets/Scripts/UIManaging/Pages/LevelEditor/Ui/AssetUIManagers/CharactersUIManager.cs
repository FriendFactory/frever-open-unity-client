using System;
using System.Linq;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.CameraSystem.CameraSystemCore;
using Modules.CharacterManagement;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.PaginationLoaders;

namespace UIManaging.Pages.LevelEditor.Ui.AssetUIManagers
{
    internal interface ICharactersUIManager
    {
        event Action<AssetSelectorsHolder> OnDisplayCharacterSelector;
        void DisplayTargetCharactersUiHandlerView(CharacterUIHandlerType handlerType, long universeId);
        BaseCharactersUIHandler GetCharacterUIHandler(CharacterUIHandlerType handlerType);
    }
    
    [UsedImplicitly]
    internal sealed class LevelEditorCharactersUIManager: CharactersUIManager
    {
        public LevelEditorCharactersUIManager(LevelEditorPageModel pageModel, ICameraSystem cameraSystem, 
            CharacterManager characterManager, ILevelManager levelManager, CharactersPaginationLoader.Factory factory, IMetadataProvider metadataProvider) 
            : base(pageModel, cameraSystem, characterManager, levelManager, factory, metadataProvider) { }
    }
    
    [UsedImplicitly]
    internal sealed class PostRecordEditorCharactersUIManager: CharactersUIManager
    {
        public PostRecordEditorCharactersUIManager(PostRecordEditorPageModel pageModel, ICameraSystem cameraSystem, 
            CharacterManager characterManager, ILevelManager levelManager, CharactersPaginationLoader.Factory factory, IMetadataProvider metadataProvider) 
            : base(pageModel, cameraSystem, characterManager, levelManager, factory, metadataProvider) { }
    }
    
    
    [UsedImplicitly]
    internal abstract class CharactersUIManager: ICharactersUIManager
    {
        public event Action<AssetSelectorsHolder> OnDisplayCharacterSelector;
        
        private readonly ILevelManager _levelManager;
        private readonly IMetadataProvider _metadataProvider;
        private readonly BaseCharactersUIHandler[] _uiHandlers;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        protected CharactersUIManager(BaseEditorPageModel pageModel, ICameraSystem cameraSystem,
                                   CharacterManager characterManager, ILevelManager levelManager, 
                                   CharactersPaginationLoader.Factory factory, IMetadataProvider metadataProvider)
        {
            _levelManager = levelManager;
            _metadataProvider = metadataProvider;
            var addCharactersUIHandler = new AddCharactersUIHandler(pageModel, levelManager, cameraSystem, factory);
            var switchCharactersUIHandler = new SwitchCharactersUIHandler(pageModel, levelManager, characterManager, factory);

            _uiHandlers = new BaseCharactersUIHandler[]
            {
                addCharactersUIHandler, 
                switchCharactersUIHandler
            };
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void DisplayTargetCharactersUiHandlerView(CharacterUIHandlerType handlerType, long universeId)
        {
            var handler = GetCharacterUIHandler(handlerType);
            SetupView(handler, universeId);
        }

        public BaseCharactersUIHandler GetCharacterUIHandler(CharacterUIHandlerType handlerType)
        {
            return _uiHandlers.First(x => x.Type == handlerType);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void SetupView(BaseCharactersUIHandler handler, long universeId)
        {
            var characterControllers = _levelManager.TargetEvent.CharacterController.ToArray();
            var selectedItems = characterControllers.Select(cc => cc.Character).ToArray();
            handler.SetupView(selectedItems, universeId);
            OnDisplayCharacterSelector?.Invoke(handler.AssetSelectorsHolder);
        }
    }
}