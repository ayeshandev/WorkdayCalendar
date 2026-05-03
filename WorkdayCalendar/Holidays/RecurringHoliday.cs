namespace WorkdayCalendar.Holidays;

public record RecurringHoliday : IHoliday
{
    public int Month { get; }
    public int Day { get; }

    private RecurringHoliday(int month, int day)
    {
        Month = month;
        Day = day;
    }

    public static RecurringHoliday Create(int month, int day)
    {
        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");
        if (day < 1 || day > 31)
            throw new ArgumentOutOfRangeException(nameof(day), "Day must be between 1 and 31.");
        if (month == 2 && day > 28)
            throw new ArgumentException("February recurring holidays must be on or before the 28th (Feb 29 is not recurring every year).");
        if ((month == 4 || month == 6 || month == 9 || month == 11) && day == 31)
            throw new ArgumentException($"Month {month} does not have 31 days.");
        return new RecurringHoliday(month, day);
    }

    public bool IsHoliday(DateOnly date) => date.Month == Month && date.Day == Day;
}
