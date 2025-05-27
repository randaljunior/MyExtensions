
namespace MyExtensions;

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
        return new DateTime(value.Year, value.Month, DateTime.DaysInMonth(value.Year, value.Month));
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
    /// Get the day of the month for a given DateTime.
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="Day"></param>
    /// <returns></returns>
    public static DateTime DayOfMonth(this DateTime dateTime, int Day)
    {
        //int _lastDay = dateTime.LastDayOfMonth().Day;
        int _lastDay = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);

        return new DateTime(dateTime.Year, dateTime.Month, (Day > _lastDay) ? _lastDay : Day);
    }

    /// <summary>
    /// Get the next weekday for a given DateTime.
    /// </summary>
    /// <param name="dateOnly"></param>
    /// <param name="feriadosBancarios">List of Bank Holydays as not working days.</param>
    /// <returns></returns>
    public static DateOnly? NextWeekDay(this DateOnly? dateOnly, HashSet<DateOnly>? feriadosBancarios = null)
    {
        if (!dateOnly.HasValue) return null;

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
    public static DateOnly LastWeekDay(this DateOnly dateOnly, IList<DateOnly>? feriadosBancarios = null)
    {
        return LastWeekDay((DateOnly?)dateOnly, feriadosBancarios?.ToHashSet()) ?? dateOnly;
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
