namespace WorkdayCalendar.Holidays
{
    public class RecurringHoliday : IHoliday
    {
        private readonly int _month;
        private readonly int _day;

        public RecurringHoliday(int month, int day)
        {
            _month = month;
            _day = day;
        }

        public bool IsHoliday(DateTime date)
        {
            return date.Month == _month && date.Day == _day;    
        }
    }
}
