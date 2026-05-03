namespace WorkdayCalendar;

public record WorkingHours
{
    public TimeOnly Start { get; }
    public TimeOnly End { get; }
    public double TotalMinutes => (End - Start).TotalMinutes;

    public WorkingHours(TimeOnly start, TimeOnly end)
    {
        if (start >= end)
            throw new ArgumentException("Workday start must be earlier than end.");
        Start = start;
        End = end;
    }

    public double MinutesFromStart(TimeOnly time) => (time - Start).TotalMinutes;
    public double MinutesFromEnd(TimeOnly time) => (End - time).TotalMinutes;
}
