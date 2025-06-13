using System;
using System.Linq;
using JetBrains.Annotations;
using UIManaging.Common.ScrollSelector;
using UIManaging.Localization;

namespace UIManaging.Pages.OnBoardingPage
{
    [UsedImplicitly]
    public sealed class BirthDayPicker
    {
        private readonly ScrollSelectorModel _yearsScrollSelectorModel;
        private readonly DateTimeLocalization _localization;
        
        private readonly int[] _availableYears;
        private int _defaultYearDataIndex;

        private int _selectedBirthYear;
        private int _selectedBirthMonth;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        public BirthDayPicker(DateTimeLocalization localization)
        {
            _localization = localization;
            
            _selectedBirthYear = GetDefaultBirthYear();
            _selectedBirthMonth = GetDefaultMonthWithPrefix();
            _availableYears = GetYearsSpan();
            
            var yearItems = _availableYears.Select(year => new ScrollSelectorItemModel {Label = year.ToString()}).ToArray();
            _yearsScrollSelectorModel = new ScrollSelectorModel(yearItems, _defaultYearDataIndex);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Reset()
        {
            _selectedBirthYear = GetDefaultBirthYear();
            _selectedBirthMonth = GetDefaultMonthWithPrefix();
        }

        public ScrollSelectorModel GetMonthsScrollSelectorModel()
        {
            var monthItems = _localization.Months.Select(month => new ScrollSelectorItemModel {Label = month}).ToArray();
            var defaultMonthDataIndex = GetDefaultMonth() - 1;
            
            return new ScrollSelectorModel(monthItems, defaultMonthDataIndex);
        }
        
        public ScrollSelectorModel GetYearsScrollSelectorModel()
        {
            return _yearsScrollSelectorModel;
        }

        public void SetBirthYear(int yearIndex)
        {
            _selectedBirthYear = _availableYears[yearIndex];
        }
        
        public void SetBirthMonth(int monthIndex)
        {
            monthIndex++;
            var month = monthIndex < 9 ? $"0{monthIndex}" : monthIndex.ToString();
            _selectedBirthMonth = int.Parse(month);
        }
        
        public DateTime GetSelectedBirthDate()
        {
            return new DateTime(_selectedBirthYear, _selectedBirthMonth, GetDaysOfMonthCount(_selectedBirthYear, _selectedBirthMonth));
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private int GetDefaultMonth()
        {
            return DateTime.Today.Month;
        }

        private int GetDefaultMonthWithPrefix()
        {
            var month = DateTime.Today.Month;
            return int.Parse($"0{month}");
        }

        private int GetDefaultBirthYear()
        {
            var year = DateTime.Today.Year;
            return year;
        }

        private int GetDaysOfMonthCount(int year, int month)
        {
            return DateTime.DaysInMonth(year, month);
        }
        
        private int[] GetYearsSpan()
        {
            const int yearsAmountToTake = 100;

            var years = new int[yearsAmountToTake];
            var maxYear = DateTime.UtcNow.Year;

            for (var i = 1; i <= years.Length; i++)
            {
                var currentYear = maxYear - (yearsAmountToTake - i);
                var yearIndex = i - 1;
                years[yearIndex] = currentYear;

                if (GetDefaultBirthYear() == currentYear)
                {
                    _defaultYearDataIndex = yearIndex;
                }
            }

            return years;
        }
    }
}
