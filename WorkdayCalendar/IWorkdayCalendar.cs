namespace WorkdayCalendar
{
    public interface IWorkdayCalendar
    {
        void SetWorkdayStartAndEnd(TimeOnly start, TimeOnly end);

        void SetRecurringHoliday(int month, int day);

        void SetSingleHoliday(DateOnly date);

        DateTime GetWorkdayIncrement(DateTime date, decimal workdays);
    }
}