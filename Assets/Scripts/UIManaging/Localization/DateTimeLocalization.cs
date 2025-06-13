using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

namespace UIManaging.Localization
{
    [CreateAssetMenu(menuName = "L10N/DateTimeLocalization", fileName = "DateTimeLocalization")]
    public class DateTimeLocalization : ScriptableObject
    {
        [SerializeField] private LocalizedString _fewMinutesAgo;
        [SerializeField] private LocalizedString _minutesVideoPostedFormat;
        [SerializeField] private LocalizedString _hoursVideoPostedFormat;
        
        [SerializeField] private LocalizedString _justNow;
        [SerializeField] private LocalizedString _minutesShortFormat;
        [SerializeField] private LocalizedString _hoursShortFormat;
        [SerializeField] private LocalizedString _daysShortFormat;
        [SerializeField] private LocalizedString _weeksShortFormat;
        
        [SerializeField] private LocalizedString _onlineNow;
        [SerializeField] private LocalizedString _onlineLongTimeAgo;
        [SerializeField] private LocalizedString _onlineSecondsShortFormat;
        [SerializeField] private LocalizedString _onlineDaysShortFormat;
        [SerializeField] private LocalizedString _onlineMinutesShortFormat;
        [SerializeField] private LocalizedString _onlineHoursShortFormat;
        
        [SerializeField] private List<LocalizedString> _months;
        
        public string FewMinutesAgo => _fewMinutesAgo;
        public string MinutesVideoPostedFormat => _minutesVideoPostedFormat;
        public string HoursVideoPostedFormat => _hoursVideoPostedFormat;

        public string JustNow => _justNow;
        public string MinutesShortFormat => _minutesShortFormat;
        public string HoursShortFormat => _hoursShortFormat;
        public string DaysShortFormat => _daysShortFormat;
        public string WeeksShortFormat => _weeksShortFormat;
        
        public string OnlineNow => _onlineNow;
        public string OnlineLongTimeAgo => _onlineLongTimeAgo;
        public string OnlineSecondsShortFormat => _onlineSecondsShortFormat;
        public string OnlineMinutesShortFormat => _onlineMinutesShortFormat;
        public string OnlineHoursShortFormat => _onlineHoursShortFormat;
        public string OnlineDaysShortFormat => _onlineDaysShortFormat;

        public List<LocalizedString> Months => _months;
    }
}
