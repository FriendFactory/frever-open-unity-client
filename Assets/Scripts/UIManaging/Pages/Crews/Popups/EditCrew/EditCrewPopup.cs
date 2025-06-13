using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bridge.Models.ClientServer;
using Bridge.Models.Common.Files;
using Extensions;
using Modules.Crew;
using TMPro;
using UIManaging.Animated.Behaviours;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Crews.Popups.EditCrew
{
    public class EditCrewPopup : BasePopup<EditCrewPopupConfiguration>
    {
        private const string PUBLIC_CREW = "Open for anyone";
        private const string PRIVATE_CREW = "Requires application";
        
        [Space]
        [SerializeField] private List<Button> _closeButtons;

        [Space] 
        [SerializeField] private EditCrewImageSection _imageSection;
        [SerializeField] private Button _editNameButton;
        [SerializeField] private TMP_Text _crewName;
        [SerializeField] private Button _descriptionButton;
        [SerializeField] private TMP_Text _crewDescription;
        [SerializeField] private Button _privacyButton;
        [SerializeField] private TMP_Text _crewPrivacy;
        [SerializeField] private Button _languageButton;
        [SerializeField] private TMP_Text _crewLanguage;
        
        [Space]
        [SerializeField] private EditCrewInputFieldPanel _namePanel;
        [SerializeField] private EditCrewInputFieldPanel _descriptionPanel;
        [SerializeField] private EditCrewPrivacyPanel _privacyPanel;
        [SerializeField] private EditCrewLanguagePanel _languagePanel;

        [Space] 
        [SerializeField] private AnimatedFullscreenOverlayBehaviour _showHideAnimation;
        [SerializeField] private AnimatedSlideInOutBehaviour _slideInOutBehaviour;

        [Inject] private CrewService _crewService;

        private CancellationTokenSource _tokenSource;
        private IEditCrewPanel _currentPanel;
        private LanguageInfo[] _languages;

        private void OnEnable()
        {
            _closeButtons.ForEach(b => b.onClick.AddListener(OnCloseButtonClicked));
            _editNameButton.onClick.AddListener(OnEditNameButtonClicked);
            _descriptionButton.onClick.AddListener(OnDescriptionClicked);
            _privacyButton.onClick.AddListener(OnPrivacyClicked);
            _languageButton.onClick.AddListener(OnLanguageClicked);

            if (Configs == null) return;
            _imageSection.ImageChangeRequested += RequestImageChange;
            _imageSection.Initialize(Configs.ThumbnailOwner);
            _showHideAnimation.PlayInAnimation(null);
        }

        private void OnDisable()
        {
            _closeButtons.ForEach(b => b.onClick.RemoveAllListeners());
            _imageSection.ImageChangeRequested -= RequestImageChange;
            _editNameButton.onClick.RemoveAllListeners();
            _descriptionButton.onClick.RemoveAllListeners();
            _privacyButton.onClick.RemoveAllListeners();
            _languageButton.onClick.RemoveAllListeners();
            
            _slideInOutBehaviour.SetInPosition();
            _currentPanel = null;
            if (_tokenSource != null)
            {
                _tokenSource.CancelAndDispose();
                _tokenSource = null;
            }
        }

        protected override async void OnConfigure(EditCrewPopupConfiguration configuration)
        {
            _tokenSource = new CancellationTokenSource();
            _crewName.text = configuration.CrewName;
            _crewDescription.text = configuration.CrewDescription;
            _crewPrivacy.text = configuration.IsPublic ? PUBLIC_CREW : PRIVATE_CREW;
            
            _languages = await _crewService.GetCrewLanguages(_tokenSource.Token);
            if (_languages == null) return;
            _crewLanguage.text = _languages.FirstOrDefault(l => l.Id == Configs.LanguageId)?.Name ?? "Not set";
            _languagePanel.Initialize(new EditCrewLanguageModel(_languages, Configs.LanguageId));
        }

        private void OnEditNameButtonClicked()
        {
            _namePanel.RequestBackAction = () => MoveBackToMainPanelAction(_namePanel);
            _namePanel.RequestCloseAction = CloseAction;
            _namePanel.RequestSaveAction = RequestNameChange;
            
            _namePanel.Initialize(Configs.CrewName);
            MoveToEditPanelAction(_namePanel);
        }

        private void OnDescriptionClicked()
        {
            _descriptionPanel.RequestBackAction = () => MoveBackToMainPanelAction(_descriptionPanel);
            _descriptionPanel.RequestCloseAction = CloseAction;
            _descriptionPanel.RequestSaveAction = RequestDescriptionChange;
            
            _descriptionPanel.Initialize(Configs.CrewDescription);
            MoveToEditPanelAction(_descriptionPanel);
        }

        private void OnLanguageClicked()
        {
            _languagePanel.RequestBackAction = () => MoveBackToMainPanelAction(_languagePanel);
            _languagePanel.RequestSaveAction += RequestLanguageChange;
            _languagePanel.Initialize(new EditCrewLanguageModel(_languages, Configs.LanguageId));
            
            MoveToEditPanelAction(_languagePanel);
        }

        private void OnPrivacyClicked()
        {
            _privacyPanel.RequestBackAction = () => MoveBackToMainPanelAction(_privacyPanel);
            _privacyPanel.RequestCloseAction = CloseAction;
            _privacyPanel.RequestSaveAction = RequestPrivacyChange;
            
            _privacyPanel.Initialize(Configs.IsPublic);
            MoveToEditPanelAction(_privacyPanel);
        }
        
        private void MoveToEditPanelAction(IEditCrewPanel panel)
        {
            panel.Show();
            _slideInOutBehaviour.PlayOutAnimation(null);
            
            _currentPanel = panel;
        }
        
        private void MoveBackToMainPanelAction(IEditCrewPanel panel)
        {
            panel.RequestBackAction = null;
            panel.RequestCloseAction = null;
            _currentPanel = null;
            
            panel.Hide();
            _slideInOutBehaviour.PlayInAnimation(null);
        }

        private void OnCloseButtonClicked()
        {
            if (_currentPanel != null)
            {
                _currentPanel.OnCloseButtonClicked();    
                
                return;
            }
            
            CloseAction();
        }

        private void CloseAction()
        {
            _showHideAnimation.PlayOutAnimation(OnHideAnimationCompleted);

            void OnHideAnimationCompleted()
            {
                Hide(null);
            }
        }

        private void RequestDescriptionChange(string description)
        {
            _crewDescription.text = description;
            Configs.UpdateCrewDescription(description);
            _crewService.UpdateCrewDescription(description);
        }

        private async void RequestLanguageChange(long id)
        {
            if (id == -1) return;

            var languages = await _crewService.GetCrewLanguages(_tokenSource.Token);
            if (languages == null) return;
            
            _crewLanguage.text = languages.FirstOrDefault(l => l.Id == id)?.Name ?? "Not set";
            var ok = await _crewService.UpdateCrewLanguageAsync(id);

            if (ok)
            {
                Configs.UpdateCrewLanguage(id);
                MoveBackToMainPanelAction(_languagePanel);
            }
            
        }

        private async void RequestNameChange(string name)
        {
            _crewName.text = name;
            Configs.UpdateCrewName(name);
            var ok = await _crewService.UpdateCrewNameAsync(name);
            
            if (ok) MoveBackToMainPanelAction(_namePanel);
        }

        private void RequestPrivacyChange(bool isPublic)
        {
            //RequestSaveAction(isPublic: isPublic);
        }

        private void RequestImageChange(List<FileInfo> fileInfo)
        {
            _crewService.UpdateCrewCoverImages(fileInfo);
        }
    }
}