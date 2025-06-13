using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;

namespace Modules.VideoStreaming.Remix.Selection
{
    internal sealed class SelectedCharacters : MonoBehaviour
    {
        public event Action<long> CharacterUnselected;
        public event Action SelectionChanged;
        
        [SerializeField] private GameObject _characterButton;
        [SerializeField] private Transform _selectedContent;

        private readonly List<SelectedCharacterButton> _selectedCharacterButtons = new List<SelectedCharacterButton>();
        private int _maxCharacters;
        private int _lastSelectedIndex;
        private long[] _originalIds;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public List<SelectedCharacterButtonModel> SelectedCharacterModels { get; } = new List<SelectedCharacterButtonModel>();

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Initialize(HashSet<long> originalIds)
        {
            _originalIds = originalIds.ToArray();
            _maxCharacters = originalIds.Count;
            CleanState();
            
            foreach (var id in originalIds)
            {
                InstantiateCharacterButton();
            }
        }

        public Dictionary<long, CharacterInfo> GetOrderedCharacters()
        {
            var characters =  new Dictionary<long, CharacterInfo>();
            var orderedSelectedCharacters = _selectedCharacterButtons.OrderBy(x => x.transform.GetSiblingIndex()).ToArray();

            for (int i = 0; i < orderedSelectedCharacters.Length; i++)
            {
                var selectedModel = SelectedCharacterModels.First(x => x.Id == orderedSelectedCharacters[i].NewId);
                characters.Add(_originalIds[i], selectedModel.CharacterInfo);
            }
            
            return characters;
        } 

        public bool AddCharacter(CharacterInfo character, Sprite image, bool canBeRemoved = true)
        {
            if (SelectedCharacterModels.Count >= _maxCharacters || character == null) return false;

            var button = _selectedCharacterButtons.First(x => x.NewId == -1);
            button.Initialize(image, character.Id, canBeRemoved);
            UpdateButtonIndices();

            _lastSelectedIndex = _selectedCharacterButtons.IndexOf(button);

            SelectedCharacterModels.Add(new SelectedCharacterButtonModel(SelectedCharacterModels.Count + 1, character));
            SelectionChanged?.Invoke();
            return true;
        }

        public bool RemoveCharacter(CharacterInfo character)
        {
            if (SelectedCharacterModels.Count < 1 || character == null) return false;
        
            var button = _selectedCharacterButtons.FirstOrDefault(x => x.NewId == character.Id);
            if (button == null) return false;

            button.ResetValues();
            button.transform.SetAsLastSibling();
            _selectedCharacterButtons.Remove(button);
            _selectedCharacterButtons.Add(button);
            var characterToRemove = SelectedCharacterModels.First(x => x.Id == character.Id);
            SelectedCharacterModels.Remove(characterToRemove);
            SelectionChanged?.Invoke();
            UpdateModelIndices();
            UpdateButtonIndices();
            return true;
        }

        public int LastSelectedCharacterIndex()
        {
            return _lastSelectedIndex;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void InstantiateCharacterButton()
        {
            var button = Instantiate(_characterButton).GetComponent<SelectedCharacterButton>();
            button.transform.SetParent(_selectedContent, false);
            button.Clicked += OnButtonClicked;
            
            button.SetIndex(_selectedCharacterButtons.Count + 1);

            _selectedCharacterButtons.Add(button);
        }

        private void OnButtonClicked(long newId)
        {
            var selectedModel = SelectedCharacterModels.FirstOrDefault(x => x.Id == newId);
            if (selectedModel == null) return;
            if (RemoveCharacter(selectedModel.CharacterInfo))
            {
                CharacterUnselected?.Invoke(newId);
            }
        }

        private void CleanState()
        {
            for (var i = 0; i < _selectedContent.childCount; i++)
            {
                Destroy(_selectedContent.GetChild(i).gameObject);
            }

            SelectedCharacterModels.Clear();
            _selectedCharacterButtons.Clear();
        }

        private void UpdateModelIndices()
        {
            for (var i = 0; i < SelectedCharacterModels.Count; i++)
            {
                SelectedCharacterModels[i].Index = i + 1;
            }
        }

        private void UpdateButtonIndices()
        {
            for (var i = 0; i < _selectedCharacterButtons.Count; i++)
            {
                _selectedCharacterButtons[i].SetIndex(i + 1);
            }
        }
    }
}
