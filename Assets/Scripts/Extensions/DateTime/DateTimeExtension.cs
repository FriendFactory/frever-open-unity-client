using System;
using System.Globalization;
using UIManaging.Localization;
using UnityEngine;

namespace Extensions.DateTime
{
    public static class DateTimeExtension
    {
        private static readonly string SHORT_DATE_FORMAT;
        private static readonly string FULL_DATE_FORMAT;
        
        private const string ONLINE_DATE_FORMAT = "yyyy-MM-dd";

        private static DateTimeLocalization _localization = 
            Resources.Load<DateTimeLocalization>("L10N/ScriptableObjects/DateTimeLocalization");
        
        static DateTimeExtension()
        {
            var formatInfo = CultureInfo.CurrentCulture.DateTimeFormat;
            FULL_DATE_FORMAT = formatInfo.ShortDatePattern.Replace(formatInfo.DateSeparator, "-");
            SHORT_DATE_FORMAT = FULL_DATE_FORMAT.Trim('y', 'Y').Trim('-');
        }

        public static string ElapsedTimeText(this System.DateTime currentTime)
        {
            var elapsed = YearsElapsed(currentTime);
            if (elapsed > 0) return currentTime.ToString(FULL_DATE_FORMAT);

            elapsed = MonthsElapsed(currentTime);
            if (elapsed > 0) return currentTime.ToString(SHORT_DATE_FORMAT);

            elapsed = WeeksElapsed(currentTime);
            if (elapsed > 0) return string.Format(_localization.WeeksShortFormat, elapsed);
            
            elapsed = DaysElapsed(currentTime);
            if (elapsed > 0) return  string.Format(_localization.DaysShortFormat, elapsed);
            
            elapsed = HoursElapsed(currentTime);
            if (elapsed > 0) return  string.Format(_localization.HoursShortFormat, elapsed);

            elapsed = MinutesElapsed(currentTime);
            if (elapsed > 0) return string.Format(_localization.MinutesShortFormat, elapsed);

            return _localization.JustNow;
        }
        
        public static string OnlineTimeText(this System.DateTime currentTime)
        {
            var elapsed = DaysElapsed(currentTime);
            if (elapsed >= 7) return _localization.OnlineLongTimeAgo;
            
            elapsed = HoursElapsed(currentTime);
            if (elapsed >= 48) return currentTime.ToString(ONLINE_DATE_FORMAT, CultureInfo.InvariantCulture);

            if (elapsed >= 24) return string.Format(_localization.OnlineDaysShortFormat, 1);
            
            if (elapsed >= 1) return string.Format(_localization.OnlineHoursShortFormat, elapsed);
            
            elapsed = MinutesElapsed(currentTime);
            if (elapsed > 0) return string.Format(_localization.OnlineMinutesShortFormat, elapsed);
            
            elapsed = SecondsElapsed(currentTime);
            if (elapsed > 0)return string.Format(_localization.OnlineSecondsShortFormat, elapsed);

            return _localization.OnlineNow;
        }

        public static string GetFormattedUntilDate(this System.DateTime time)
        {
            time = time.ToLocalTime();
            return time.ToString(time.DayOfYear == System.DateTime.Now.DayOfYear ? "hh:mm" : "MM/dd hh:mm", CultureInfo.InvariantCulture);
        }
        
        public static string GetFormattedTimeLeft(this System.DateTime endDate)
        {
            var timeSpan = endDate - System.DateTime.UtcNow;
            if (timeSpan > TimeSpan.FromDays(1))
            {
                return $"{string.Format(_localization.DaysShortFormat, (int)timeSpan.TotalDays)}" +
                       $" {string.Format(_localization.HoursShortFormat, timeSpan.Hours)}";
            }
            
            if (timeSpan > TimeSpan.FromHours(1))
            {
                return $"{string.Format(_localization.HoursShortFormat, timeSpan.Hours)}" +
                       $" {string.Format(_localization.MinutesShortFormat, timeSpan.Minutes)}";
            }

            return string.Format(_localization.MinutesShortFormat,
                                 Math.Max((int)Math.Ceiling(timeSpan.TotalMinutes), 2));
        }
        
        public static string GetVideoPostedFormattedTime(this System.DateTime dateTimeUtc)
        {
            var timeDifference = System.DateTime.UtcNow - dateTimeUtc;

            if (timeDifference.TotalMinutes < 5)
            {
                return _localization.FewMinutesAgo;
            }

            if (timeDifference.TotalHours < 1)
            {
                return string.Format(_localization.MinutesVideoPostedFormat, timeDifference.TotalMinutes.ToString("F0"));
            }

            if (timeDifference.TotalHours < 24)
            {
                return string.Format(_localization.HoursVideoPostedFormat, timeDifference.TotalHours.ToString("F0"));
            }

            return $"{dateTimeUtc.ToString("MMM d, yyyy", CultureInfo.InvariantCulture)}";
        }

        private static int SecondsElapsed(System.DateTime time)
        {
            return (int)(System.DateTime.UtcNow - time).TotalSeconds;
        }
        
        private static int MinutesElapsed(System.DateTime time)
        {
            return (int)(System.DateTime.UtcNow - time).TotalMinutes;
        }
        
        private static int HoursElapsed(System.DateTime time)
        {
            return (int)(System.DateTime.UtcNow - time).TotalHours;
        }
        
        private static int DaysElapsed(System.DateTime time)
        {
            return (int)(System.DateTime.UtcNow - time).TotalDays;
        }
        
        private static int WeeksElapsed(System.DateTime time)
        {
            return DaysElapsed(time)/7;
        }
        
        private static int MonthsElapsed(System.DateTime time)
        {
            return DaysElapsed(time)/30;
        }
        
        private static int YearsElapsed(System.DateTime time)
        {
            return DaysElapsed(time)/365;
        }

        public static UserAge Age(this System.DateTime dateOfBirth)
        {
            var now = System.DateTime.UtcNow;
            
            if (dateOfBirth > now) return new UserAge();
            
            var monthDay = new int[] { 31, -1, 31, 30, 31, 30, 31, 31, 30, 31, 30,
                31 };
            
            var day = 0;
            var month = 0;
            var year = 0;
            var increment = 0; 
            
            if (dateOfBirth.Day > now.Day)
            { 
                increment = monthDay[dateOfBirth.Month - 1]; 
            }
            
            if (increment == -1)
            {
                increment = System.DateTime.IsLeapYear(dateOfBirth.Year) ? 29 : 28;
            }
            
            if (increment != 0)
            {    
                day = now.Day + increment - dateOfBirth.Day;
                increment = 1; 
            }
            else
            {       
                day = now.Day - dateOfBirth.Day;
            }
            
            if (dateOfBirth.Month + increment > now.Month)
            {   
                month = now.Month + 12 - (dateOfBirth.Month + increment);
                increment = 1;
            }
            else
            {    
                month = now.Month - (dateOfBirth.Month + increment);
                increment = 0;
            }
            
            year = now.Year - (dateOfBirth.Year + increment);

            return new UserAge
            {
                Days = day,
                Months = month,
                Years = year
            };
        }
    }

    public struct UserAge
    {
        public int Years;
        public int Months;
        public int Days;
    }
}
