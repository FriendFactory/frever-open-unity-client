using System.Linq;
using AdvancedInputFieldPlugin;
using Bridge;
using Bridge.VideoServer;
using Navigation.Core;
using TMPro;
using UIManaging.Common.Args.Buttons;
using UIManaging.Common.InputFields;
using UIManaging.Common.PageHeader;
using UIManaging.Localization;
using UIManaging.Pages.ReportPage.Ui.Args;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.ReportPage.Ui
{
    public class ReportPage : GenericPage<ReportPageArgs>
    {
        public override PageId Id { get; } = PageId.ReportPage;

        [SerializeField] private PageHeaderView _pageHeaderView;
        [SerializeField] private Button _submitButton;
        [SerializeField] private TextMeshProUGUI _descriptionLimitText;
        [SerializeField] private TMP_Dropdown _reasonDropdown;
        [SerializeField] private AdvancedInputField _descriptionInputField;
        [SerializeField] private ReportReasonDropdown _reportReasonDropdown;
        [SerializeField] private CharacterLimitFilter _characterLimitFilter;

        [Inject] private IBridge _bridge;
        [Inject] private PopupManager _popupManager;
        [Inject] private InputFieldAdapterFactory _inputFieldAdapterFactory;
        [Inject] private FeedLocalization _localization;
        [Inject] private VideoReportReasonMapping _reportReasonLocalization;
        
        private ReportPageArgs _pageArgs;
        private PageManager _pageManager;
        private VideoReportReason[] _videoReportReasons;
        private IInputFieldAdapter _inputFieldAdapter;
        
        protected override void OnInit(PageManager pageManager)
        {
            _pageManager = pageManager;
            _inputFieldAdapter = _inputFieldAdapterFactory.CreateInstance(_descriptionInputField);
        }
        
        protected override void OnDisplayStart(ReportPageArgs args)
        {
            base.OnDisplayStart(args);
            _pageArgs = args;
            _reasonDropdown.options.Clear();
            RefreshDescriptionLimitText();
            DownloadReportReasons();
            // under the hood adapter adds one to set character limit in order to show popup in other cases
            _pageHeaderView.Init(new PageHeaderArgs(null, new ButtonArgs(string.Empty, OnBackButtonClicked)));
            Reset();
            _submitButton.interactable = false;
        }

        private void OnBackButtonClicked()
        {
            _pageManager.MoveBack();
        }

        private void RefreshDescriptionLimitText()
        {
            _descriptionLimitText.text = $"{_characterLimitFilter.CharacterLimit - _inputFieldAdapter.Text.Length}";
        }

        private void OnEnable()
        {
            _submitButton.onClick.AddListener(OnSubmitButtonClicked);
            _reasonDropdown.onValueChanged.AddListener(OnReasonDropdownValueChanged);
            _inputFieldAdapter.OnValueChanged += OnDescriptionInputValueChanged;
        }

        private void OnDisable()
        {
            _submitButton.onClick.RemoveListener(OnSubmitButtonClicked);
            _reasonDropdown.onValueChanged.RemoveListener(OnReasonDropdownValueChanged);
            _inputFieldAdapter.OnValueChanged -= OnDescriptionInputValueChanged;
            _reasonDropdown.gameObject.SetActive(false);
        }

        private async void OnSubmitButtonClicked()
        {
            _submitButton.interactable = false;
            var optionIndex = _reasonDropdown.value;
            var reasonId = _videoReportReasons[optionIndex].Id;
            var reportData = new ReportData
            {
                Message = _inputFieldAdapter.Text, 
                ReasonId = reasonId,
            };

            var reportVideo = await _bridge.Report(_pageArgs.Video.Id, reportData);
            if (reportVideo.IsSuccess)
            {
                OnReportSuccess();
            }
            else
            {
                Debug.LogError( $"Failed to send report. [Reason]: {reportVideo.ErrorMessage}");
            }
            _submitButton.interactable = true;
        }

        private void OnDescriptionInputValueChanged(string text)
        {
            RefreshDescriptionLimitText();
        }

        private void OnReasonDropdownValueChanged(int value)
        {
            _submitButton.interactable = value >= 0;
        }

        private void OnReportSuccess()
        {
            _pageManager.MoveBack();
            CreateSuccessPopup();
        }

        private void Reset()
        {
            _inputFieldAdapter.Text = string.Empty;
            _reasonDropdown.value = 0;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            _inputFieldAdapter.Dispose();
        }

        private void CreateSuccessPopup()
        {
            var popupConfig = new AlertPopupConfiguration()
            {
                PopupType = PopupType.AlertPopup,
                Description = _localization.ReportVideoSuccessPopupDescription,
                Title = _localization.ReportVideoSuccessPopupTitle,
                ConfirmButtonText = _localization.ReportVideoSuccessPopupConfrimButton
            };
            _popupManager.SetupPopup(popupConfig);
            _popupManager.ShowPopup(PopupType.AlertPopup);
        }

        private async void DownloadReportReasons()
        {
            var downloadReportReasons = await _bridge.GetReportReasons();
            if (downloadReportReasons.IsSuccess)
            {
                _videoReportReasons = downloadReportReasons.Models;
                foreach (var reportReason in downloadReportReasons.Models)
                {
                    _reasonDropdown.options.Add(new TMP_Dropdown.OptionData(_reportReasonLocalization.GetLocalized(reportReason.Name)));
                }
                _reasonDropdown.gameObject.SetActive(true);

                _reasonDropdown.value = -1;
                
                _reportReasonDropdown.UpdateItemColors();
            }
            else
            {
                Debug.LogError($"Failed to download video report reasons: [Reason]: {downloadReportReasons.ErrorMessage}");
            }
            
        }
    }
}