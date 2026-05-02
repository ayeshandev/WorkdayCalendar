namespace WorkdayCalendar.Holidays
{
    public interface IHoliday
    {
        bool IsHoliday(DateOnly date);
    }
}