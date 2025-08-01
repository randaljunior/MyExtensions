using System.Runtime.CompilerServices;

namespace MyExtensions;

public static class DateTimeExtensions
{
    /// <summary>
    /// Coalesce a nullable DateTime to DateTime.MaxValue if null.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime CoalesceMax(this DateTime? value)
    {
        return value ?? DateTime.MaxValue;
    }

    /// <summary>
    /// Coalesce a nullable DateTime to DateTime.MinValue if null.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime CoalesceMin(this DateTime? value)
    {
        return value ?? DateTime.MinValue;
    }

    /// <summary>
    /// Get the first day of the month for a given DateTime.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime FirstDayOfMonth(this DateTime value)
    {
        return (value.Day == 1) ? value : new DateTime(value.Year, value.Month, 1);
    }

    /// <summary>
    /// Get the first day of the month for a given DateTime.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateOnly FirstDayOfMonth(this DateOnly value)
    {
        return (value.Day == 1) ? value : new DateOnly(value.Year, value.Month, 1);
    }

    /// <summary>
    /// Get the last day of the month for a given DateTime.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime LastDayOfMonth(this DateTime value)
    {
        var _lastDayOfMonth = DateTime.DaysInMonth(value.Year, value.Month);
        return (value.Day == _lastDayOfMonth) ? value : new DateTime(value.Year, value.Month, _lastDayOfMonth);
    }

    /// <summary>
    /// Get the last day of the month for a given DateTime.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateOnly LastDayOfMonth(this DateOnly value)
    {
        var _lastDayOfMonth = DateTime.DaysInMonth(value.Year, value.Month);
        return (value.Day == _lastDayOfMonth) ? value : new DateOnly(value.Year, value.Month, _lastDayOfMonth);
    }

    /// <summary>
    /// Calculate the percentage of the month between two DateTime values.
    /// </summary>
    /// <param name="firstDate"></param>
    /// <param name="nextDate"></param>
    /// <returns></returns>
    public static decimal MonthPercentage(this DateTime firstDate, DateTime nextDate)
    {
        DateTime FirstValue, LastValue;

        if (firstDate <= nextDate)
            (FirstValue, LastValue) = (firstDate, nextDate);
        else
            (FirstValue, LastValue) = (nextDate, firstDate);

        //DateTime FirstValueLD = FirstValue.LastDayOfMonth();
        //decimal PercToEndMonthBegin = (decimal)(FirstValueLD - FirstValue).TotalDays/FirstValueLD.Day;
        int FirstValueLD = DateTime.DaysInMonth(FirstValue.Year, FirstValue.Month);
        decimal PercToEndMonthBegin = (decimal)(FirstValueLD - FirstValue.Day) / FirstValueLD;

        //DateTime LastValueLD = LastValue.LastDayOfMonth();
        //decimal PercToEndMonthEnd = (decimal)(LastValueLD - LastValue).TotalDays/ LastValueLD.Day;
        int LastValueLD = DateTime.DaysInMonth(LastValue.Year, LastValue.Month);
        decimal PercToEndMonthEnd = (decimal)(LastValueLD - LastValue.Day) / LastValueLD;

        int months = (LastValue.Month - FirstValue.Month) + (LastValue.Year - FirstValue.Year) * 12;

        return PercToEndMonthBegin - PercToEndMonthEnd + months;
    }

    /// <summary>
    /// Calculate the percentage of the month between two DateTime values.
    /// </summary>
    /// <param name="firstDate"></param>
    /// <param name="nextDate"></param>
    /// <returns></returns>
    public static decimal MonthPercentage(this DateOnly firstDate, DateOnly nextDate)
    {
        return MonthPercentage(firstDate.ToDateTime(), nextDate.ToDateTime());
    }

    /// <summary>
    /// Get the day of the month for a given DateTime.
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="Day"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime DayOfMonth(this DateTime dateTime, int Day)
    {
        //int _lastDay = dateTime.LastDayOfMonth().Day;
        int _lastDay = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);

        return new DateTime(dateTime.Year, dateTime.Month, (Day > _lastDay) ? _lastDay : Day);
    }

    /// <summary>
    /// Get the day of the month for a given DateTime.
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="Day"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateOnly DayOfMonth(this DateOnly dateTime, int Day)
    {
        //int _lastDay = dateTime.LastDayOfMonth().Day;
        int _lastDay = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);

        return new DateOnly(dateTime.Year, dateTime.Month, (Day > _lastDay) ? _lastDay : Day);
    }

    /// <summary>
    /// Get the next weekday for a given DateTime.
    /// </summary>
    /// <param name="dateOnly"></param>
    /// <param name="feriadosBancarios">List of Bank Holydays as not working days.</param>
    /// <returns></returns>
    public static DateOnly? NextWeekDay(this DateOnly? dateOnly, HashSet<DateOnly>? feriadosBancarios = null)
    {
        if (!dateOnly.HasValue)
            return null;

        DateOnly result = dateOnly.Value;

        while (true)
        {
            // Verifica se é fim de semana usando um switch para lidar com os dois casos de forma direta.
            switch (result.DayOfWeek)
            {
                case DayOfWeek.Saturday:
                    result = result.AddDays(2); // Sábado pula para segunda-feira.
                    continue;
                case DayOfWeek.Sunday:
                    result = result.AddDays(1); // Domingo pula para segunda-feira.
                    continue;
            }

            // Se houver feriado declarado para essa data, incrementa um dia.
            if (feriadosBancarios is not null && feriadosBancarios.Contains(result))
            {
                result = result.AddDays(1);
                continue;
            }

            // Se não for fim de semana nem feriado, sai do loop.
            break;
        }

        return dateOnly;
    }

    /// <summary>
    /// Get the next weekday for a given DateTime.
    /// </summary>
    /// <param name="dateOnly"></param>
    /// <param name="feriadosBancarios">List of Bank Holydays as not working days.</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateOnly? NextWeekDay(this DateOnly? dateOnly, IList<DateOnly>? feriadosBancarios = null)
    {
        return NextWeekDay(dateOnly, feriadosBancarios?.ToHashSet()) ?? dateOnly;
    }

    /// <summary>
    /// Get the next weekday for a given DateTime.
    /// </summary>
    /// <param name="dateOnly"></param>
    /// <param name="feriadosBancarios">List of Bank Holydays as not working days.</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateOnly NextWeekDay(this DateOnly dateOnly, HashSet<DateOnly>? feriadosBancarios = null)
    {
        return NextWeekDay((DateOnly?)dateOnly, feriadosBancarios) ?? dateOnly;
    }

    /// <summary>
    /// Get the next weekday for a given DateTime.
    /// </summary>
    /// <param name="dateOnly"></param>
    /// <param name="feriadosBancarios">List of Bank Holydays as not working days.</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateOnly NextWeekDay(this DateOnly dateOnly, IList<DateOnly>? feriadosBancarios = null)
    {
        return NextWeekDay((DateOnly?)dateOnly, feriadosBancarios?.ToHashSet()) ?? dateOnly;
    }

    /// <summary>
    /// Get the last month weekday for a given DateTime.
    /// </summary>
    /// <param name="dateOnly"></param>
    /// <param name="feriadosBancarios">List of Bank Holydays as not working days.</param>
    /// <returns></returns>
    public static DateOnly? LastWeekDay(this DateOnly? dateOnly, HashSet<DateOnly>? feriadosBancarios = null)
    {
        if (!dateOnly.HasValue)
            return null;

        DateOnly result = dateOnly.Value;

        while (true)
        {
            // Usa o switch para tratar diretamente os fins de semana e minimizar iterações.
            switch (result.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    // Se é domingo, sabemos que o dia anterior (sábado) também é fim de semana,
                    // assim podemos descer direto para sexta-feira.
                    result = result.AddDays(-2);
                    continue;

                case DayOfWeek.Saturday:
                    // Se é sábado, apenas subtrai um dia para chegar à sexta.
                    result = result.AddDays(-1);
                    continue;
            }

            // Se a data for feriado, subtrai um dia e reavalia.
            if (feriadosBancarios != null && feriadosBancarios.Contains(result))
            {
                result = result.AddDays(-1);
                continue;
            }

            // Se não for final de semana nem feriado, a data é considerada válida.
            break;
        }

        return dateOnly;
    }

    /// <summary>
    /// Get the last month weekday for a given DateTime.
    /// </summary>
    /// <param name="dateOnly"></param>
    /// <param name="feriadosBancarios">List of Bank Holydays as not working days.</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateOnly? LastWeekDay(this DateOnly? dateOnly, IList<DateOnly>? feriadosBancarios = null)
    {
        return LastWeekDay(dateOnly, feriadosBancarios?.ToHashSet()) ?? dateOnly;
    }

    /// <summary>
    /// Get the last month weekday for a given DateTime.
    /// </summary>
    /// <param name="dateOnly"></param>
    /// <param name="feriadosBancarios">List of Bank Holydays as not working days.</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateOnly LastWeekDay(this DateOnly dateOnly, HashSet<DateOnly>? feriadosBancarios = null)
    {
        return LastWeekDay((DateOnly?)dateOnly, feriadosBancarios) ?? dateOnly;
    }

    /// <summary>
    /// Get the last month weekday for a given DateTime.
    /// </summary>
    /// <param name="dateOnly"></param>
    /// <param name="feriadosBancarios">List of Bank Holydays as not working days.</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateOnly LastWeekDay(this DateOnly dateOnly, IList<DateOnly>? feriadosBancarios = null)
    {
        return LastWeekDay((DateOnly?)dateOnly, feriadosBancarios?.ToHashSet()) ?? dateOnly;
    }

    /// <summary>
    /// Convert a DateTime to DateOnly.
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateOnly ToDateOnly(this DateTime date)
    {
        return DateOnly.FromDateTime(date);
    }

    /// <summary>
    /// Convet a DateOnly to DateTime with the time set to midnight (00:00:00).
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime ToDateTime(this DateOnly date)
    {
        return date.ToDateTime(new TimeOnly(0, 0));
    }

    /// <summary>
    /// Convert a DateOnly to DateTime with the specified time.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime ToDateTime(this DateOnly date, TimeOnly time)
    {
        return new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
    }

    /// <summary>
    /// Convert a DateOnly and TimeOnly to DateTime in the specified time zone.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="time"></param>
    /// <param name="timeZone"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime ToDateTime(this DateOnly date, TimeOnly time, TimeZoneInfo timeZone)
    {
        var dateTime = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
        return TimeZoneInfo.ConvertTime(dateTime, timeZone);
    }

    /// <summary>
    /// Convert a DateOnly and TimeOnly to DateTime in the specified time zone by its ID.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="time"></param>
    /// <param name="timeZoneId"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime ToDateTime(this DateOnly date, TimeOnly time, string timeZoneId)
    {
        var dateTime = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return TimeZoneInfo.ConvertTime(dateTime, timeZone);
    }

    /// <summary>
    /// Compare two DateTime values and return the maximum.
    /// </summary>
    /// <param name="firstDate"></param>
    /// <param name="secondDate"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime Max(this DateTime firstDate, DateTime secondDate)
    {
        return firstDate > secondDate ? firstDate : secondDate;
    }

    /// <summary>
    /// Compare two DateTime values and return the minimum.
    /// </summary>
    /// <param name="firstDate"></param>
    /// <param name="secondDate"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime Min(this DateTime firstDate, DateTime secondDate)
    {
        return firstDate < secondDate ? firstDate : secondDate;
    }

    /// <summary>
    /// Compare two DateOnly values and return the maximum.
    /// </summary>
    /// <param name="firstDate"></param>
    /// <param name="secondDate"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateOnly Max(this DateOnly firstDate, DateOnly secondDate)
    {
        return firstDate > secondDate ? firstDate : secondDate;
    }

    /// <summary>
    /// Compare two DateOnly values and return the minimum.
    /// </summary>
    /// <param name="firstDate"></param>
    /// <param name="secondDate"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateOnly Min(this DateOnly firstDate, DateOnly secondDate)
    {
        return firstDate < secondDate ? firstDate : secondDate;
    }
}
