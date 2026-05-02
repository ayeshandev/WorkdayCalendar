namespace WorkdayCalendar.Holidays
{
    public interface IHoliday
    {
        bool IsHoliday(DateTime date);
    }
}