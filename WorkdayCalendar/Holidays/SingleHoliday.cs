namespace WorkdayCalendar.Holidays;

/// <summary>
/// Represents a single-instance holiday on a specific date.
/// This is used for holidays that occur only once, such as a special national day.
/// </summary>
/// <param name="Date">The specific date of this holiday.</param>
public record SingleHoliday(DateOnly Date) : IHoliday
{
    /// <summary>
    /// Determines whether the specified date matches this single holiday.
    /// </summary>
    /// <param name="date">The date to check.</param>
    /// <returns>true if the date equals this holiday's date; otherwise, false.</returns>
    public bool IsHoliday(DateOnly date) => Date == date;
}
