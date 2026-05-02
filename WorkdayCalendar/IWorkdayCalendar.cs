namespace WorkdayCalendar
{
    public interface IWorkdayCalendar
    {
        void SetWorkdayStartAndEnd();

        void SetRecurringHoliday();

        void SetSingleHoliday();

        DateTime GetWorkdayIncrement(DateTime date, decimal workdays);
    }
}