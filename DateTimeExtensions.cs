using System;
using System.Collections.Generic;
using System.Text;

namespace MyExtensions
{
    public static class DateTimeExtensions
    {
        public static DateTime CoalesceMax(this DateTime? value)
        {
            return value ?? DateTime.MaxValue;
        }

        public static DateTime CoalesceMin(this DateTime? value)
        {
            return value ?? DateTime.MinValue;
        }

        public static DateTime FirstDayOfMonth(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, 1);
        }

        public static DateTime LastDayOfMonth(this DateTime value)
        {
            var date = value.AddMonths(1);
            return new DateTime(date.Year, date.Month, 1).AddDays(-1);
        }

        public static decimal MonthPercentage(this DateTime value, DateTime nextDate)
        {
            DateTime FirstValue, LastValue;

            if (value <= nextDate) (FirstValue, LastValue) = (value, nextDate);
            else (FirstValue, LastValue) = (nextDate, value);

            DateTime FirstValueLD = FirstValue.LastDayOfMonth();
            decimal PercToEndMonthBegin = (decimal)(FirstValueLD - FirstValue).TotalDays/FirstValueLD.Day;

            DateTime LastValueLD = LastValue.LastDayOfMonth();
            decimal PercToEndMonthEnd = (decimal)(LastValueLD - LastValue).TotalDays/ LastValueLD.Day;

            int months = (LastValue.Month - FirstValue.Month) + (LastValue.Year - FirstValue.Year) * 12;

            return PercToEndMonthBegin - PercToEndMonthEnd + months;
        }

        public static DateTime DayOfMonth(this DateTime dateTime, int Day)
        {
            int _lastDay = dateTime.LastDayOfMonth().Day;

            return new DateTime(dateTime.Year, dateTime.Month, (Day > _lastDay) ? _lastDay : Day);
        }

    }
}
