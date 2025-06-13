using System.Linq;
using Bridge;
using Extensions;
using JetBrains.Annotations;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.LevelManagement;
using UnityEngine;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.Characters
{
    internal class CharactersSelectionPanel: MonoBehaviour
    {
        [SerializeField] private GameObject _body;
        [SerializeField] protected BaseCharacterSelectionElement[] _characterButtons;

        private IBridge _bridge;
        protected ILevelManager LevelManager;
        protected BaseEditorPageModel PageModel;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject, UsedImplicitly]
        private void Construct(IBridge bridge, ILevelManager levelManager, BaseEditorPageModel pageModel)
        {
            _bridge = bridge;
            LevelManager = levelManager;
            PageModel = pageModel;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            LevelManager.CharacterSpawned += OnCharacterSpawned;
            LevelManager.CharacterReplaced += OnCharacterReplaced;
            LevelManager.CharacterDestroyed += OnCharacterDestroyed;
            
            foreach (var characterButton in _characterButtons)
            {
                characterButton.OnClickedEvent += OnClicked;
            }
            
            _body.SetActive(true);
        }

        private void OnDisable()
        {
            LevelManager.CharacterSpawned -= OnCharacterSpawned;
            LevelManager.CharacterReplaced -= OnCharacterReplaced;
            LevelManager.CharacterDestroyed -= OnCharacterDestroyed;
            
            foreach (var characterButton in _characterButtons)
            {
                characterButton.OnClickedEvent -= OnClicked;
            }
            
            _body.SetActive(false);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        protected virtual void OnClicked(long characterId)
        {
            PageModel.OnCharacterButtonClicked(characterId);
        }

        public async void UpdateCharacterButtons()
        {
            var targetEvent = LevelManager.TargetEvent;
            if (targetEvent == null) return;

            var characterControllers = targetEvent.CharacterController.ToArray();
            for (var i = 0; i < _characterButtons.Length; i++)
            {
                var button = _characterButtons[i];
                var isActive = i < characterControllers.Length;

                if (!isActive)
                {
                    SetCharacterViewActive(button, false);
                    continue;
                }

                var character = characterControllers[i].Character;
                button.Character = character.ToCharacterInfo();

                var thumbnailInfo = character.Files.First(x => x.Resolution == Resolution._128x128);
                var result = await _bridge.GetCharacterThumbnailAsync(character.Id, thumbnailInfo);
                
                isActive = i < targetEvent.CharacterController.ToArray().Length;

                if (!isActive)
                {
                    SetCharacterViewActive(button, false);
                    continue;
                }
                
                if (result.IsSuccess)
                {
                    button.Thumbnail.sprite = (result.Object as Texture2D).ToSprite();
                    SetCharacterViewActive(button, true);
                }
                else
                {
                    Debug.LogWarning(result.ErrorMessage);
                    SetCharacterViewActive(button, false);
                }
            }

            OnCharactersUpdated();
        }

        protected virtual void SetCharacterViewActive(BaseCharacterSelectionElement characterSelectionElement, bool value)
        {
            characterSelectionElement.SetActive(value);
        }

        protected virtual void OnCharactersUpdated()
        {
            
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnCharacterSpawned(ICharacterAsset characterAsset)
        {
            UpdateCharacterButtons();
        }

        private void OnCharacterReplaced(ICharacterAsset characterAsset)
        {
            UpdateCharacterButtons();
        }

        private void OnCharacterDestroyed()
        {
            SetCurrentCharacterAsTargetForSwitching();
            UpdateCharacterButtons();
        }

        private void SetCurrentCharacterAsTargetForSwitching()
        {
            var targetCharacter = LevelManager.TargetEvent.GetTargetCharacterController().CharacterId;
            PageModel.SetSwitchTargetCharacterId(targetCharacter);
        }
    }
}