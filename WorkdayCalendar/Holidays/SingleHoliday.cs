namespace WorkdayCalendar.Holidays
{
    public class SingleHoliday : IHoliday
    {
        private readonly DateOnly _date;

        public SingleHoliday(DateOnly date) 
        {
            _date = date;
        }

        public bool IsHoliday(DateTime date)
        {
            return DateOnly.FromDateTime(date).Equals(_date);
        }
    }
}
