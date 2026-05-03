namespace WorkdayCalendar;

/// <summary>
/// Represents the working hours for a business day.
/// This is a value object that encapsulates the start and end times of a workday.
/// </summary>
public record WorkingHours
{
    /// <summary>
    /// Gets the start time of the working day.
    /// </summary>
    public TimeOnly Start { get; }

    /// <summary>
    /// Gets the end time of the working day.
    /// </summary>
    public TimeOnly End { get; }

    /// <summary>
    /// Gets the total duration of the working day in minutes.
    /// </summary>
    public double TotalMinutes => (End - Start).TotalMinutes;

    /// <summary>
    /// Initializes a new instance of the WorkingHours record.
    /// </summary>
    /// <param name="start">The start time of the working day.</param>
    /// <param name="end">The end time of the working day.</param>
    /// <exception cref="ArgumentException">Thrown when start time is not earlier than end time.</exception>
    public WorkingHours(TimeOnly start, TimeOnly end)
    {
        if (start >= end)
            throw new ArgumentException("Workday start must be earlier than end.");
        Start = start;
        End = end;
    }

    /// <summary>
    /// Calculates the number of minutes from the start of the working day to the given time.
    /// </summary>
    /// <param name="time">The time to calculate from.</param>
    /// <returns>The number of minutes from Start to the given time.</returns>
    public double MinutesFromStart(TimeOnly time) => (time - Start).TotalMinutes;

    /// <summary>
    /// Calculates the number of minutes from the given time to the end of the working day.
    /// </summary>
    /// <param name="time">The time to calculate from.</param>
    /// <returns>The number of minutes from the given time to End.</returns>
    public double MinutesFromEnd(TimeOnly time) => (End - time).TotalMinutes;
}
