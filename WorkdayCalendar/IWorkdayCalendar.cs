namespace WorkdayCalendar
{
    /// <summary>
    /// Provides functionality to calculate workdays, accounting for weekends and holidays.
    /// </summary>
    public interface IWorkdayCalendar
    {
        /// <summary>
        /// Defines the working hours for a business day.
        /// </summary>
        /// <param name="start">The start time of the working day.</param>
        /// <param name="end">The end time of the working day.</param>
        /// <remarks>
        /// The start time must be earlier than the end time. Default is 08:00 to 16:00.
        /// </remarks>
        void SetWorkdayStartAndEnd(TimeOnly start, TimeOnly end);

        /// <summary>
        /// Adds a recurring holiday that occurs on the same date every year.
        /// </summary>
        /// <param name="month">The month (1-12) of the recurring holiday.</param>
        /// <param name="day">The day (1-31) of the recurring holiday.</param>
        /// <remarks>
        /// February 29 cannot be set as a recurring holiday since it does not occur every year.
        /// </remarks>
        void SetRecurringHoliday(int month, int day);

        /// <summary>
        /// Adds a single-instance holiday on a specific date.
        /// </summary>
        /// <param name="date">The date to mark as a holiday.</param>
        void SetSingleHoliday(DateOnly date);

        /// <summary>
        /// Calculates a new datetime by adding or subtracting a specified number of working days.
        /// </summary>
        /// <param name="date">The starting datetime.</param>
        /// <param name="workdays">The number of workdays to add (positive) or subtract (negative). Fractional workdays are supported.</param>
        /// <returns>
        /// A datetime that results from moving forward or backward by the specified number of workdays.
        /// The resulting date is guaranteed to be a workday, and the time is within working hours.
        /// </returns>
        /// <remarks>
        /// If the input datetime falls outside working hours, it is normalized to the nearest working time.
        /// If the input date is a weekend or holiday, it is moved to the next or previous workday respectively.
        /// </remarks>
        DateTime GetWorkdayIncrement(DateTime date, decimal workdays);
    }
}