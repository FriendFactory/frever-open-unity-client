using System;
using System.Linq;
using Bridge;
using Extensions.DateTime;
using Modules.Amplitude;
using Modules.SignUp;
using Navigation.Core;
using UIManaging.Common.ScrollSelector;
using UIManaging.Localization;
using UIManaging.Pages.OnBoardingPage.UI;
using UiManaging.Pages.OnBoardingPage.UI.Args;
using UIManaging.PopupSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UiManaging.Pages.OnBoardingPage.UI.Pages
{
    public class OnBoardingBirthdayPage : OnBoardingBasePage<OnBoardingScrollSelectorPageArgs>
    {
        // private const int MIN_AGE = 13;
        private const int MAX_AGE = 100;

        [SerializeField] private GameObject _customiseSection;
        [SerializeField] private Button _customiseButton;
        [SerializeField] private ScrollSelectorView _monthScrollView;
        [SerializeField] private ScrollSelectorView _yearScrollView;

        [Inject] private IBridge _bridge;
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private PopupManager _popupManager;
        [Inject] private OnBoardingLocalization _localization;
        [Inject] private DateTimeLocalization _dateTimeLocalization;
        [Inject] private AmplitudeManager _amplitudeManager;
        [Inject] private SignUpService _signUpService;

        private MonthScrollModel _monthScrollModel;
        private YearScrollModel _yearScrollModel;

        private int _selectedMonth;
        private int _selectedYear;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override PageId Id => PageId.OnBoardingScrollSelectorPage;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected override void OnEnable()
        {
            base.OnEnable();
            
            _customiseButton.onClick.AddListener(OnCustomise);
            _monthScrollView.OnScrollerSnappedEvent += OnMonthScrollerSnapped;
            _yearScrollView.OnScrollerSnappedEvent += OnYearScrollerSnapped;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            _customiseButton.onClick.RemoveListener(OnCustomise);
            _monthScrollView.OnScrollerSnappedEvent -= OnMonthScrollerSnapped;
            _yearScrollView.OnScrollerSnappedEvent -= OnYearScrollerSnapped;
        }
        
        protected override void OnDisplayStart(OnBoardingScrollSelectorPageArgs args)
        {
            base.OnDisplayStart(args);

            _monthScrollModel = new MonthScrollModel(_dateTimeLocalization);
            _monthScrollView.Initialize(_monthScrollModel);

            _yearScrollModel = new YearScrollModel(MAX_AGE);
            _yearScrollView.Initialize(_yearScrollModel);

            _selectedMonth = _monthScrollModel.InitialDataIndex + 1;
            _selectedYear = int.Parse(_yearScrollModel.Items[_yearScrollModel.InitialDataIndex].Label);
            
            _customiseSection.SetActive(Application.platform == RuntimePlatform.Android);
            
            ContinueButton.interactable = true;
        }

        protected override void OnContinueButtonClicked()
        {
            var ageOfConsent = _signUpService.CountryInfo.AgeOfConsent;
            if (!CheckMinimalAge(ageOfConsent))
            {
                _amplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.USER_UNDER_AGE_OF_CONSENT);
                return;
            }
            
            OpenPageArgs.RequestMoveNext?.Invoke(_selectedMonth, _selectedYear);
            
            _bridge.SaveToken();
            ContinueButton.interactable = false;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnCustomise()
        {
            var config = new DataPrivacyOverlayConfiguration();
            
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType);
        }

        private void OnMonthScrollerSnapped(int dataIndex)
        {
            _selectedMonth = dataIndex + 1;
            ContinueButton.interactable = true;
        }

        private void OnYearScrollerSnapped(int dataIndex)
        {
            _selectedYear = int.Parse(_yearScrollModel.Items[dataIndex].Label);
            ContinueButton.interactable = true;
        }

        private bool CheckMinimalAge(int ageOfConsent)
        {
            // Consider that the user is born on the last day of the selected month as a worst-case scenario
            var daysInSelectedMonth = DateTime.DaysInMonth(_selectedYear, _selectedMonth);
            var selectedDate = new DateTime(_selectedYear, _selectedMonth, daysInSelectedMonth, 23, 59, 59);
            var age = selectedDate.Age();

            if (age.Years < 0)
            {
                _popupManagerHelper.ShowAlertPopup(_localization.BirthdateFutureAgeMessage);
                return false;
            }

            if (age.Years < ageOfConsent)
            {
                _popupManagerHelper.ShowAlertPopup(string.Format(_localization.BirthdateMinAgeMessage, ageOfConsent));
                return false;
            }

            return true;
        }

        //---------------------------------------------------------------------
        // Nested
        //---------------------------------------------------------------------

        private sealed class MonthScrollModel : IScrollSelectorModel
        {
            private const int DEFAULT_MONTH_INDEX = 0;
            
            public ScrollSelectorItemModel[] Items { get; }
            public int InitialDataIndex { get; }

            public MonthScrollModel(DateTimeLocalization localization)
            {
                Items = localization.Months.Select(month => new ScrollSelectorItemModel {Label = month}).ToArray();
                InitialDataIndex = DEFAULT_MONTH_INDEX;
            }
        }

        private sealed class YearScrollModel : IScrollSelectorModel
        {
            private const string DEFAULT_YEAR = "2010";
            
            public ScrollSelectorItemModel[] Items { get; }
            public int InitialDataIndex { get; }

            public YearScrollModel(int maxAge)
            {
                Items = GetYearItems(maxAge);
                InitialDataIndex = Array.FindIndex(Items, y => y.Label == DEFAULT_YEAR);;
            }

            private ScrollSelectorItemModel[] GetYearItems(int maxAge)
            {
                var oldestAvailableYear = DateTime.Now.Year - maxAge;

                // Have to add current year to include current year
                var years = Enumerable.Range(oldestAvailableYear, maxAge + 1);
                return years
                      .Select(year => new ScrollSelectorItemModel { Label = year.ToString() })
                      .ToArray();
            }
        }
    }
}