using System;
using System.Linq;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Localization;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.UserSelection
{
    public class UserSelectionPage : GenericPage<UserSelectionPageArgs>
    {
        private const int MAX_SELECTED = 100;
        
        public override PageId Id => PageId.UserSelectionPage;

        [SerializeField] private Button _backButton;
        [SerializeField] private Button _saveButton;
        [SerializeField] private TextMeshProUGUI _titleCounter;

        [SerializeField] private UserSelectionWidget _userSelectionWidget;

        [Inject] private PublishPageLocalization _localization;
        
        private UserSelectionPanelModel _userSelectionPanelModel;

        private void OnEnable()
        {
            _backButton.onClick.AddListener(OnBackButton);
            _saveButton.onClick.AddListener(OnSaveButton);
        }

        private void OnDisable()
        {
            _backButton.onClick.RemoveListener(OnBackButton);
            _saveButton.onClick.RemoveListener(OnSaveButton);
        }
        
        protected override void OnInit(PageManager pageManager)
        {
            
        }

        protected override void OnDisplayStart(UserSelectionPageArgs args)
        {
            base.OnDisplayStart(args);

            if (_userSelectionPanelModel != null)
            {
                _userSelectionPanelModel.ItemSelectionChanged -= OnItemSelectionChanged;
                _userSelectionPanelModel.Clear();
            }

            var selectedModels = args.SelectedProfiles.Select(profile => new UserSelectionItemModel(profile)).ToList();
            var lockedModels = selectedModels.Where(model => args.LockedProfiles.Any(profile => profile.Id == model.Id))
                                             .Concat(args.LockedProfiles
                                                         .Where(profile => selectedModels.All(
                                                                    model => model.Id != profile.Id))
                                                         .Select(profile => new UserSelectionItemModel(profile)))
                                             .ToList();
            
            _userSelectionPanelModel = new UserSelectionPanelModel(MAX_SELECTED, 
                                                                   selectedModels, 
                                                                   lockedModels,
                                                                   args.TargetProfileId,
                                                                   args.Filter);
            
            _userSelectionPanelModel.ItemSelectionChanged += OnItemSelectionChanged;
            
            _userSelectionWidget.Initialize(_userSelectionPanelModel);
            
            UpdateCounter();
            _saveButton.interactable = _userSelectionPanelModel.SelectedItems.Count > 0;
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            _userSelectionWidget.CleanUp();
            
            base.OnHidingBegin(onComplete);
        }

        private void OnBackButton()
        {
            OpenPageArgs.OnBackButton?.Invoke();
        }

        private void OnSaveButton()
        {
            OpenPageArgs.OnSaveButton?.Invoke(_userSelectionPanelModel.SelectedItems.Select(item => item.ShortProfile).ToList());
        }

        private void OnItemSelectionChanged(UserSelectionItemModel item)
        {
            UpdateCounter();
            _saveButton.interactable = _userSelectionPanelModel.SelectedItems.Count > 0;
        }

        private void UpdateCounter()
        {
            _titleCounter.text = string.Format(_localization.UserSelectionTitleFormat,
                                               _userSelectionPanelModel.SelectedItems.Count, MAX_SELECTED);
        }
    }
}