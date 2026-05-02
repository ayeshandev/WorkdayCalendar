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
            if(workdays == 0)
            {
                return date;
            }

            bool forwardIncrement = workdays > 0;
            var (effectiveDate, effectiveTime) = GetEffectiveDateTime(date, forwardIncrement);

            var offsetMinutes = GetOffsetMinutes(effectiveTime, forwardIncrement);

            var workdayMinutes = GetWorkdayMinutes();

            var totalWorkdayMinutes = Math.Abs((double)workdays * workdayMinutes);

            var totalMinutes = offsetMinutes + totalWorkdayMinutes;

            int totalFullWorkdays = (int)Math.Floor(totalMinutes / workdayMinutes);
            var remainingMinutes = (int)Math.Floor(totalMinutes % workdayMinutes);

            var finalDate = forwardIncrement ? GetIncrementedDate(effectiveDate, totalFullWorkdays) : GetDecrementedDate(effectiveDate, totalFullWorkdays);

            var finalDateTime = forwardIncrement ? _workdayStart.Add(TimeSpan.FromMinutes(remainingMinutes)) : _workdayEnd.Add(-1 * TimeSpan.FromMinutes(remainingMinutes));

            return finalDate.ToDateTime(finalDateTime);
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

        private (DateOnly date, TimeOnly time) GetEffectiveDateTime(DateTime date, bool forwardIncrement)
        {
            var effectiveDate = DateOnly.FromDateTime(date);
            var effectiveTime = TimeOnly.FromDateTime(date);

            if (forwardIncrement)
            {
                if (effectiveTime < _workdayStart)
                {
                    effectiveTime = _workdayStart;
                }
                else if (effectiveTime > _workdayEnd)
                {
                    effectiveTime = _workdayStart;
                    effectiveDate = effectiveDate.AddDays(1);
                }
            }
            else
            {
                if (effectiveTime < _workdayStart)
                {
                    effectiveTime = _workdayEnd;
                    effectiveDate = effectiveDate.AddDays(-1);
                }
                else if (effectiveTime > _workdayEnd)
                {
                    effectiveTime = _workdayEnd;
                }
            }

            return (effectiveDate, effectiveTime);
        }

        private double GetOffsetMinutes(TimeOnly effectiveTime, bool forwardIncrement)
        {
            return forwardIncrement ? (effectiveTime - _workdayStart).TotalMinutes : (_workdayEnd - effectiveTime).TotalMinutes;
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

        private DateOnly PreviousWorkday(DateOnly date)
        {
            var previousDate = date.AddDays(-1);
            while (!IsWorkday(previousDate))
            {
                previousDate = previousDate.AddDays(-1);
            }
            return previousDate;
        }

        private DateOnly GetIncrementedDate(DateOnly effectiveDate, int totalFullWorkdays)
        {

            int addedWorkdays = 0;
            var incrementedDate = effectiveDate;

            while (addedWorkdays < totalFullWorkdays)
            {
                incrementedDate = NextWorkday(incrementedDate);
                addedWorkdays++;
            }

            return incrementedDate;
        }

        private DateOnly GetDecrementedDate(DateOnly effectiveDate, int totalFullWorkdays)
        {

            int deductedWorkdays = 0;
            var decrementedDate = effectiveDate;

            while (deductedWorkdays < totalFullWorkdays)
            {
                decrementedDate = PreviousWorkday(decrementedDate);
                deductedWorkdays++;
            }

            return decrementedDate;
        }
    }
}
