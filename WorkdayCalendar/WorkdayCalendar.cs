using WorkdayCalendar.Holidays;

namespace WorkdayCalendar
{
    public class WorkdayCalendar : IWorkdayCalendar
    {
        private readonly List<IHoliday> _holidays = new();
        private TimeOnly _workdayStart = new TimeOnly(8, 0);
        private TimeOnly _workdayEnd = new TimeOnly(16, 0);

        public DateTime GetWorkdayIncrement(DateTime date, decimal workdays)
        {
            throw new NotImplementedException();
        }

        public void SetRecurringHoliday(int month, int day)
        {
            _holidays.Add(new RecurringHoliday(month, day));
        }

        public void SetSingleHoliday(DateOnly date)
        {
            _holidays.Add(new SingleHoliday(date));
        }

        public void SetWorkdayStartAndEnd(TimeOnly start, TimeOnly end)
        {
            _workdayStart = start;
            _workdayEnd = end;
        }
    }
}
