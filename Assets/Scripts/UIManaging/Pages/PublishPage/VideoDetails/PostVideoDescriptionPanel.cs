using System;
using Bridge.Services.UserProfile;
using Common.Abstract;
using UIManaging.Common.InputFields;
using UIManaging.Common.SearchPanel;
using UIManaging.Pages.SharingPage.Ui;
using UnityEngine;

namespace UIManaging.Pages.PublishPage.VideoDetails
{
    internal class PostVideoDescriptionPanel: BaseContextlessPanel
    {
        [SerializeField] private DescriptionPanel _descriptionPanel;
        [SerializeField] private IgnoredDeselectableAreaAdvancedInputField _descriptionInputField;
        [SerializeField] private SearchHandler _mentionsSearchHandler;

        private bool _isSelectedOnce;

        //---------------------------------------------------------------------
        // Properties 
        //---------------------------------------------------------------------
        
        public bool IsCharLimitExceeded => _descriptionPanel.IsCharLimitExceeded;
        public string ParsedDescriptionText => _descriptionPanel.GetParsedText();
        
        public string RawDescriptionText
        {
            get => _descriptionPanel.InputFieldAdapter.Text;
            set => _descriptionPanel.InputFieldAdapter.Text = value ?? string.Empty;
        }
        
        public string DescriptionPlaceholder
        {
            get => _descriptionInputField.PlaceHolderText;
            set => _descriptionInputField.PlaceHolderText = value;
        }
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action InputFieldSelected;
        public event Action InputFieldDeselected;
        public event Action<bool> CharacterLimitExceededStatusChanged;
        public event Action<string> VideoDescriptionUpdated;
        public event Action<Profile> MentionSelected;
        
        //---------------------------------------------------------------------
        // Protected 
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            _descriptionPanel.InputFieldAdapter.OnDeselect += OnInputFieldDeselected;
            _descriptionPanel.InputFieldAdapter.OnSelect += OnInputFieldSelected;
            _descriptionPanel.CharacterLimitExceededStatusChanged += OnCharacterLimitExceededStatusChanged;
            
            _mentionsSearchHandler.ProfileButtonClicked += OnMentionSelected;

            if (_isSelectedOnce) return;
            
            _isSelectedOnce = true;
            _descriptionPanel.InputFieldAdapter.Select();
        }

        protected override void BeforeCleanUp()
        {
            _descriptionPanel.InputFieldAdapter.OnDeselect -= OnInputFieldDeselected;
            _descriptionPanel.InputFieldAdapter.OnSelect -= OnInputFieldSelected;
            _descriptionPanel.CharacterLimitExceededStatusChanged -= OnCharacterLimitExceededStatusChanged;
            
            _mentionsSearchHandler.ProfileButtonClicked -= OnMentionSelected;
        }

        private void OnInputFieldDeselected(string description)
        {
            VideoDescriptionUpdated?.Invoke(description);
            InputFieldDeselected?.Invoke();
        }
        
        private void OnCharacterLimitExceededStatusChanged(bool isExceeded)
        {
            CharacterLimitExceededStatusChanged?.Invoke(isExceeded);
        }

        private void OnInputFieldSelected()
        {
            InputFieldSelected?.Invoke();
        }

        private void OnMentionSelected(Profile profile)
        {
            MentionSelected?.Invoke(profile);
        }
    }
}