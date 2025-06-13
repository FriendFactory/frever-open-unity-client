using System;
using System.Collections.Generic;
using System.Linq;
using Modules.VideoStreaming.Remix.Selection;
using UIManaging.Pages.Feed.Remix.Loaders;

namespace UIManaging.Pages.Feed.Remix.Collection
{
    internal abstract class BaseCharacterSelectionListModel: IDisposable
    {
        protected readonly SelectedCharacters SelectedCharactersManager;
        protected readonly IPaginatedCharactersLoader PaginatedCharactersLoader;
        protected readonly List<CharacterButtonModel> _models;
        
        private readonly bool _canDeselectCharacters;
        
        //---------------------------------------------------------------------
        // Properties 
        //---------------------------------------------------------------------

        public bool AwaitingData { get; protected set; }
        public IReadOnlyList<CharacterButtonModel> Models => _models;
        
        //---------------------------------------------------------------------
        // Events 
        //---------------------------------------------------------------------

        public event Action NewPageAppended;
        public event Action LastPageLoaded;
        
        //---------------------------------------------------------------------
        // Ctors 
        //---------------------------------------------------------------------
        
        protected BaseCharacterSelectionListModel(SelectedCharacters selectedCharactersManager, IPaginatedCharactersLoader paginatedCharactersLoader, bool canDeselectCharacters)
        {
            SelectedCharactersManager = selectedCharactersManager;
            PaginatedCharactersLoader = paginatedCharactersLoader;
            _canDeselectCharacters = canDeselectCharacters;
            
            _models = new List<CharacterButtonModel>();

            SelectedCharactersManager.CharacterUnselected += CharacterUnselected;
            
            PaginatedCharactersLoader.NewPageAppended += OnNewPageAppended;
            PaginatedCharactersLoader.LastPageLoaded += OnLastPageLoaded;
        }
        
        //---------------------------------------------------------------------
        // Public 
        //---------------------------------------------------------------------
        
        public virtual async void DownloadNextPage()
        {
            AwaitingData = true;
            
            await PaginatedCharactersLoader.DownloadNextPageAsync();
            
            AwaitingData = false;
        }

        public virtual void Dispose()
        {
            _models.ForEach(model => model.Dispose());

            SelectedCharactersManager.CharacterUnselected -= CharacterUnselected;
            
            PaginatedCharactersLoader.NewPageAppended -= OnNewPageAppended;
            PaginatedCharactersLoader.LastPageLoaded -= OnLastPageLoaded;
        }
        
        //---------------------------------------------------------------------
        // Protected 
        //---------------------------------------------------------------------

        protected abstract void OnNewPageAppendedInternal();
        protected abstract void AddCharacterModel(DownloadedCharacterModel model);

        protected void ProcessCharacterSelection(CharacterButtonModel characterModel)
        {
            if (characterModel.Selected)
            {
                UnselectCharacterButton(characterModel);
            }
            else
            {
                SelectCharacterButton(characterModel);
            }
        }
        
        //---------------------------------------------------------------------
        // Helpers 
        //---------------------------------------------------------------------

        private void OnNewPageAppended()
        {
            OnNewPageAppendedInternal();
            
            NewPageAppended?.Invoke();
        }

        private void OnLastPageLoaded()
        {
            PaginatedCharactersLoader.NewPageAppended -= OnNewPageAppended;
            PaginatedCharactersLoader.LastPageLoaded -= OnLastPageLoaded;
            
            LastPageLoaded?.Invoke();
        }

        private void SelectCharacterButton(CharacterButtonModel characterModel)
        {
            var character = characterModel.Character;
            var thumbnail = characterModel.Thumbnail;

            var isCharacterAdded = SelectedCharactersManager.AddCharacter(character, thumbnail, _canDeselectCharacters);
            if (!isCharacterAdded) return;

            var borderCount = SelectedCharactersManager.LastSelectedCharacterIndex() + 1;
            characterModel.SetBorderCount(borderCount);
            characterModel.Select();
        }

        private void UnselectCharacterButton(CharacterButtonModel characterModel)
        {
            var isCharacterRemoved = SelectedCharactersManager.RemoveCharacter(characterModel.Character);
            if (!isCharacterRemoved) return;

            characterModel.Unselect();
            RefreshBorderCounters();
        }
        
        private void CharacterUnselected(long id)
        {
            var characterModel = _models.FirstOrDefault(x => x.Id == id);
            
            characterModel?.Unselect();
            RefreshBorderCounters();
        }

        private void RefreshBorderCounters()
        {
            foreach (var characterModel in _models)
            {
                if (!characterModel.Selected) continue;
                var newBorderCount = SelectedCharactersManager 
                                    .SelectedCharacterModels
                                    .FirstOrDefault(x => x.Id == characterModel.Id)?.Index ?? -1;
                
                characterModel.SetBorderCount(newBorderCount);
            }
        }
    }
}