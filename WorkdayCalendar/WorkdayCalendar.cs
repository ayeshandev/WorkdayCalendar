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
            // Consider only forward increments for now, negative increments can be added later
            var (effectiveDate, effectiveTime) = GetEffectiveDateTime(date);

            var offsetMinutes = (effectiveTime - _workdayStart).TotalMinutes;

            var workdayMinutes = GetWorkdayMinutes();

            var totalWorkdayMinutes = (double)workdays * workdayMinutes;

            var totalMinutes = offsetMinutes + totalWorkdayMinutes;

            int totalFullWorkdays = (int)Math.Floor(totalMinutes / workdayMinutes);
            var remainingMinutes = totalMinutes % workdayMinutes;

            int addedWorkdays = 0;
            var incrementedDate = effectiveDate;

            while (addedWorkdays < totalFullWorkdays)
            {
                incrementedDate = NextWorkday(incrementedDate);
                addedWorkdays++;
            }

            var incrementedDateTime = _workdayStart.Add(TimeSpan.FromMinutes(remainingMinutes));

            return incrementedDate.ToDateTime(incrementedDateTime);
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

        private bool IsWorkday(DateOnly date)
        {
            return !IsWeekend(date) && !IsHoliday(date);
        }

        private bool IsWeekend(DateOnly date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }

        private bool IsHoliday(DateOnly date)
        {
            return _holidays.Any(h => h.IsHoliday(date));
        }

        private double GetWorkdayMinutes()
        {
            return (_workdayEnd - _workdayStart).TotalMinutes;
        }

        private (DateOnly date, TimeOnly time) GetEffectiveDateTime(DateTime date)
        {
            var effectiveDate = DateOnly.FromDateTime(date);
            var effectiveTime = TimeOnly.FromDateTime(date);
            if (effectiveTime < _workdayStart)
            {
                effectiveTime = _workdayStart;
            }
            else if (effectiveTime > _workdayEnd)
            {
                effectiveTime = _workdayStart;
                effectiveDate = effectiveDate.AddDays(1);
            }
            return (effectiveDate, effectiveTime);
        }

        private DateOnly NextWorkday(DateOnly date)
        {
            var nextDate = date.AddDays(1);
            while (!IsWorkday(nextDate))
            {
                nextDate = nextDate.AddDays(1);
            }
            return nextDate;
        }
    }
}
