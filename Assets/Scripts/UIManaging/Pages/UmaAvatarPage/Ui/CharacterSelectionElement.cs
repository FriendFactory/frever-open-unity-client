using AdvancedInputFieldPlugin;
using Extensions;
using System;
using System.Runtime.ConstrainedExecution;
using TMPro;
using UIManaging.Common.InputFields;
using UIManaging.Pages.Common.Helpers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.UmaAvatarPage.Ui
{
    public class CharacterSelectionElement : MonoBehaviour
    {
        private const int CHARACTER_LIMIT = 25;

        [SerializeField] private RawImage _image;
        [SerializeField] private Button _editButton;
        [SerializeField] private Button _deleteButton;
        [SerializeField] private AdvancedInputField _characterNameInputField;

        private Action<string> _nameChanged;
        private string _characterName;

        public void Initialize(Texture2D thumbnail, string characterName, Action deleteAction, Action editAction, Action<string> nameChanged)
        {
            Clear();
            if (thumbnail != null)
            {
                _image.gameObject.SetActive(true);
                _image.texture = thumbnail;
            }
            _editButton.onClick.AddListener(()=>editAction());
            _deleteButton.onClick.AddListener(()=>deleteAction());
            _nameChanged = nameChanged;
            _characterName = characterName;
            _characterNameInputField.Text = characterName;
            _characterNameInputField.OnEndEdit.AddListener((text, reason)=> OnEndEdit(text, reason));
            _characterNameInputField.CharacterLimit = CHARACTER_LIMIT;
        }

        public void UpdateThumbnail(Texture2D thumbnail)
        {
            _image.gameObject.SetActive(true);
            _image.texture = thumbnail;
        }

        private void Clear()
        {
            _editButton.onClick.RemoveAllListeners();
            _deleteButton.onClick.RemoveAllListeners();
            _characterNameInputField.OnEndEdit.RemoveAllListeners();
        }

        private void OnEndEdit(string newName, EndEditReason endReason) 
        {
            var isNewNameValid = !string.IsNullOrWhiteSpace(newName);
            if (isNewNameValid && endReason != EndEditReason.KEYBOARD_CANCEL) 
            {
                _characterName = newName;
                _nameChanged(newName);
            }
            else 
            {
                _characterNameInputField.Text = _characterName;
            }
            
        }
    }
}
