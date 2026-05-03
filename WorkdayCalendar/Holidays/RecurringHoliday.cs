namespace WorkdayCalendar.Holidays;

/// <summary>
/// Represents a recurring holiday that occurs on the same date every year.
/// This is used for holidays like Christmas, Independence Day, etc.
/// </summary>
public record RecurringHoliday : IHoliday
{
    /// <summary>
    /// Gets the month of the recurring holiday (1-12).
    /// </summary>
    public int Month { get; }

    /// <summary>
    /// Gets the day of the recurring holiday (1-31).
    /// </summary>
    public int Day { get; }

    /// <summary>
    /// Initializes a new instance of the RecurringHoliday record.
    /// </summary>
    /// <param name="month">The month of the holiday.</param>
    /// <param name="day">The day of the holiday.</param>
    private RecurringHoliday(int month, int day)
    {
        Month = month;
        Day = day;
    }

    /// <summary>
    /// Creates a new recurring holiday with validation.
    /// </summary>
    /// <param name="month">The month (1-12) of the recurring holiday.</param>
    /// <param name="day">The day (1-31) of the recurring holiday.</param>
    /// <returns>A new RecurringHoliday instance if validation passes.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when month is outside 1-12 or day is outside 1-31.</exception>
    /// <exception cref="ArgumentException">Thrown when the date is invalid (e.g., Feb 29, April 31) or February 29 is attempted.</exception>
    public static RecurringHoliday Create(int month, int day)
    {
        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");
        if (day < 1 || day > 31)
            throw new ArgumentOutOfRangeException(nameof(day), "Day must be between 1 and 31.");
        // Prevent February 29 as recurring holiday since it doesn't occur every year
        if (month == 2 && day > 28)
            throw new ArgumentException("February recurring holidays must be on or before the 28th (Feb 29 is not recurring every year).");
        // Validate that the month actually has the specified day
        if ((month == 4 || month == 6 || month == 9 || month == 11) && day == 31)
            throw new ArgumentException($"Month {month} does not have 31 days.");
        return new RecurringHoliday(month, day);
    }

    /// <summary>
    /// Determines whether the specified date matches this recurring holiday.
    /// Compares only the month and day, ignoring the year.
    /// </summary>
    /// <param name="date">The date to check.</param>
    /// <returns>true if the month and day match this recurring holiday; otherwise, false.</returns>
    public bool IsHoliday(DateOnly date) => date.Month == Month && date.Day == Day;
}
