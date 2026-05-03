namespace WorkdayCalendar.Holidays;

public record SingleHoliday(DateOnly Date) : IHoliday
{
    public bool IsHoliday(DateOnly date) => Date == date;
}
