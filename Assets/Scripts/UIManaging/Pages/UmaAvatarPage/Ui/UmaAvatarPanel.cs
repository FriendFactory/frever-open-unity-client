using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;

namespace UIManaging.Pages.UmaAvatarPage.Ui 
{
    public sealed class UmaAvatarPanel : MonoBehaviour
    {
        [SerializeField] private Transform _contentView;
        [SerializeField] private GameObject _avatarViewPrefab;
        [SerializeField] private Button _addCharacterViewPrefab;

        private readonly List<UmaAvatarView> _avatarViews = new();

        private Action<CharacterInfo> _onCharacterClicked;
        private Action _onAddCharacterClicked;
        private Button _addCharacterView;
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Setup(Action<CharacterInfo> onCharacterClicked, Action onAddCharacterClicked)
        {
            _onCharacterClicked = onCharacterClicked;
            _onAddCharacterClicked = onAddCharacterClicked;
        }

        public void SetupGrid(IEnumerable<CharacterInfo> characters)
        {
            foreach (var character in characters)
            {
                AddCharacter(character);
            }
            CreateAddView();
        }

        public void ClearCharacters() 
        {
            _avatarViews.ForEach(av => Destroy(av.gameObject));
            _avatarViews.Clear();
            if (_addCharacterView != null)
            {
                Destroy(_addCharacterView.gameObject);
                _addCharacterView = null;
            }
        }

        public void DeleteCharacter(CharacterInfo character) 
        {
            var view = _avatarViews.Find(x=> x.Character == character);
            if (view == null) return;

            _avatarViews.Remove(view);
            Destroy(view.gameObject);
        }

        public void SelectCharacter(CharacterInfo character) 
        {
            _avatarViews.ForEach(avatarView => 
            {
                avatarView.SetCharacterUsing(false);
                if (character != null && avatarView.Character.Id == character.Id)
                {
                    avatarView.SetCharacterUsing(true);
                }
            });
        }

        public void UpdateCharacter(CharacterInfo character)
        {
            var characterView = _avatarViews.Find(view => view.Character.Id == character.Id);
            if (characterView != null)
            {
                characterView.Setup(character, OnClicked);
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void AddCharacter(CharacterInfo character) 
        {
            var view = Instantiate(_avatarViewPrefab, _contentView).GetComponent<UmaAvatarView>();   
            view.Setup(character, OnClicked);
            _avatarViews.Add(view);
        }

        private void OnClicked(CharacterInfo character)
        {
            _onCharacterClicked?.Invoke(character);
        }

        private void CreateAddView()
        {
            var view = Instantiate(_addCharacterViewPrefab, _contentView);
            view.onClick.AddListener(OnAddCharacterClicked);
            _addCharacterView = view;
        }

        private void OnAddCharacterClicked()
        {
            _onAddCharacterClicked?.Invoke();
        }
    }
}
