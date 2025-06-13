using System;
using UnityEngine;

namespace Common.TimeManaging
{
    public static class TimeSpanExtensions
    {
        public static string ToFormattedString(this TimeSpan timeSpan)
        {
            if (timeSpan > TimeSpan.FromDays(1))
            {
                return $"{(int)Math.Ceiling(timeSpan.TotalDays)} days";
            }
            
            if (timeSpan > TimeSpan.FromHours(1))
            {
               return $"{(int)Math.Ceiling(timeSpan.TotalHours)} hours";
            }
                
            return $"{Mathf.Max((int)Math.Ceiling(timeSpan.TotalMinutes), 2)} mins";
        }

        public static string ToHoursMinutesString(this TimeSpan timeSpan)
        {
            if (timeSpan > TimeSpan.FromHours(1))
            {
                return $"{(int)Math.Floor(timeSpan.TotalHours)}h{timeSpan.Minutes}m";
            }
            
            return $"{Mathf.Max(timeSpan.Minutes, 2)}m";
        }
    }
}