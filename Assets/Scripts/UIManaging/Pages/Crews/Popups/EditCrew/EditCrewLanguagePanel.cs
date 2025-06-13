using System;
using Abstract;
using Bridge.Models.ClientServer;
using Extensions;
using UIManaging.Animated.Behaviours;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Crews.Popups.EditCrew
{
    internal sealed class EditCrewLanguageModel
    {
        public long CurrentLanguage;
        public LanguageInfo[] Languages;

        public EditCrewLanguageModel(LanguageInfo[] languages, long languageId)
        {
            Languages = languages;
            CurrentLanguage = languageId;
        }
        
    }

    internal sealed class EditCrewLanguagePanel : BaseContextDataView<EditCrewLanguageModel>, IEditCrewPanel
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _doneButton;
        [SerializeField] private LanguageCloudView _languageCloud;
        
        [Space] 
        [SerializeField] private AnimatedSlideInOutBehaviour _slideInOut;

        [Inject] private PopupManager _popupManager;
        [Inject] private PopupManagerHelper _popupManagerHelper;
        
         private long _selectedLanguage = -1;


        public Action RequestBackAction { get; set; }
        public Action RequestCloseAction { get; set; }
        public Action<long> RequestSaveAction;

         private void OnEnable()
         {
             _backButton.onClick.AddListener(OnBackButtonClicked);
             _doneButton.onClick.AddListener(OnDoneButtonClicked);
         }

         private void OnDisable()
         {
             _backButton.onClick.RemoveAllListeners();
             _doneButton.onClick.RemoveAllListeners();
             
             _selectedLanguage = -1;
             CleanUp();
         }
         
        public void Show()
        {
            gameObject.SetActive(true);
            _languageCloud.SetActive(true);
            _languageCloud.Initialize(new LanguageCloudModel(ContextData.Languages, ContextData.CurrentLanguage));
            _languageCloud.LanguageSelected += OnLanguageSelected;
            _slideInOut.PlayInAnimation(null);
        }

        public void Hide()
        {
            _languageCloud.LanguageSelected -= OnLanguageSelected;
            _slideInOut.PlayOutAnimation(OnAnimationCompleted);

            void OnAnimationCompleted()
            {
                gameObject.SetActive(false);
            }
        }

         protected override void OnInitialized()
        {
            _languageCloud.Initialize(new LanguageCloudModel(ContextData.Languages, ContextData.CurrentLanguage));
            _languageCloud.SetActive(false);
        }

         private void OnLanguageSelected(long id)
         {
             _selectedLanguage = id;         
         }

         private void OnBackButtonClicked()
         {
             if (_selectedLanguage == -1 || _selectedLanguage == ContextData.CurrentLanguage)
             {
                 RequestBackAction?.Invoke();
                 return;
             }
             
             _popupManagerHelper.ShowEraseChangesPopup(OnEraseClicked, OnKeepEditingClicked);

             void OnEraseClicked() => RequestBackAction?.Invoke();
             void OnKeepEditingClicked() => _popupManager.ClosePopupByType(PopupType.DialogDarkV3Vertical);
         }
         

         private void OnDoneButtonClicked()
         {
             RequestSaveAction?.Invoke(_selectedLanguage);
         }


        public void OnCloseButtonClicked()
        {
            RequestBackAction?.Invoke();
        }
    }

}