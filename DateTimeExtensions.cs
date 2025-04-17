using System;
using System.Collections.Generic;
using System.Text;

namespace MyExtensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Coalesce a nullable DateTime to DateTime.MaxValue if null.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime CoalesceMax(this DateTime? value)
        {
            return value ?? DateTime.MaxValue;
        }

        /// <summary>
        /// Coalesce a nullable DateTime to DateTime.MinValue if null.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime CoalesceMin(this DateTime? value)
        {
            return value ?? DateTime.MinValue;
        }

        /// <summary>
        /// Get the first day of the month for a given DateTime.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime FirstDayOfMonth(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, 1);
        }

        /// <summary>
        /// Get the last day of the month for a given DateTime.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime LastDayOfMonth(this DateTime value)
        {
            var date = value.AddMonths(1);
            return new DateTime(date.Year, date.Month, 1).AddDays(-1);
        }

        /// <summary>
        /// Calculate the percentage of the month between two DateTime values.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="nextDate"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get the day of the month for a given DateTime.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="Day"></param>
        /// <returns></returns>
        public static DateTime DayOfMonth(this DateTime dateTime, int Day)
        {
            int _lastDay = dateTime.LastDayOfMonth().Day;

            return new DateTime(dateTime.Year, dateTime.Month, (Day > _lastDay) ? _lastDay : Day);
        }

        /// <summary>
        /// Get the next weekday for a given DateTime.
        /// </summary>
        /// <param name="dateOnly"></param>
        /// <param name="feriadosBancarios">List of Bank Holydays as not working days.</param>
        /// <returns></returns>
        public static DateOnly? NextWeekDay(this DateOnly? dateOnly, List<DateOnly>? feriadosBancarios = null)
        {
            if (dateOnly is null) return null;

            while (
                dateOnly?.DayOfWeek == DayOfWeek.Saturday
                || dateOnly?.DayOfWeek == DayOfWeek.Sunday
                || (feriadosBancarios is not null && feriadosBancarios.Contains(dateOnly!.Value)))
            {
                dateOnly = dateOnly?.AddDays(1);
            }

            return dateOnly;
        }

        /// <summary>
        /// Get the next weekday for a given DateTime.
        /// </summary>
        /// <param name="dateOnly"></param>
        /// <param name="feriadosBancarios">List of Bank Holydays as not working days.</param>
        /// <returns></returns>
        public static DateOnly NextWeekDay(this DateOnly dateOnly, List<DateOnly>? feriadosBancarios = null)
        {
            return NextWeekDay((DateOnly?)dateOnly, feriadosBancarios) ?? dateOnly;
        }

        /// <summary>
        /// Get the last month weekday for a given DateTime.
        /// </summary>
        /// <param name="dateOnly"></param>
        /// <param name="FeriadosBancarios">List of Bank Holydays as not working days.</param>
        /// <returns></returns>
        public static DateOnly? LastWeekDay(this DateOnly? dateOnly, List<DateOnly>? FeriadosBancarios = null)
        {
            if (dateOnly is null) return null;

            while (
                dateOnly?.DayOfWeek == DayOfWeek.Saturday
                || dateOnly?.DayOfWeek == DayOfWeek.Sunday
                || (FeriadosBancarios is not null && FeriadosBancarios.Contains(dateOnly!.Value)))
            {
                dateOnly = dateOnly?.AddDays(-1);
            }

            return dateOnly;
        }

        /// <summary>
        /// Get the last month weekday for a given DateTime.
        /// </summary>
        /// <param name="dateOnly"></param>
        /// <param name="FeriadosBancarios">List of Bank Holydays as not working days.</param>
        /// <returns></returns>
        public static DateOnly LastWeekDay(this DateOnly dateOnly, List<DateOnly>? FeriadosBancarios = null)
        {
            return LastWeekDay((DateOnly?)dateOnly, FeriadosBancarios) ?? dateOnly;
        }

        /// <summary>
        /// Convert a DateTime to DateOnly.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateOnly ToDateOnly(this DateTime date)
        {
            return DateOnly.FromDateTime(date);
        }
    }
}
