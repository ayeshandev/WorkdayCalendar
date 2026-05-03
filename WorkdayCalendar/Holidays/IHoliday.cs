namespace WorkdayCalendar.Holidays
{
    /// <summary>
    /// Represents an abstraction for different types of holidays.
    /// </summary>
    public interface IHoliday
    {
        /// <summary>
        /// Determines whether the specified date is a holiday according to this holiday definition.
        /// </summary>
        /// <param name="date">The date to check.</param>
        /// <returns>true if the date is a holiday; otherwise, false.</returns>
        bool IsHoliday(DateOnly date);
    }
}