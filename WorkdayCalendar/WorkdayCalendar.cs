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
            var (effectiveDate, effectiveTime) = forwardIncrement ? GetForwardEffectiveDateTime(date) : GetBackwardEffectiveDateTime(date);

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
            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");

            if (day < 1 || day > 31)
                throw new ArgumentOutOfRangeException(nameof(day), "Day must be between 1 and 31.");

            // February: disallow 29, 30, 31 because recurring holidays should be valid every year
            if (month == 2 && day > 28)
                throw new ArgumentException("February recurring holidays must be on or before the 28th (Feb 29 is not recurring every year).");

            // Months with 30 days: April(4), June(6), September(9), November(11)
            if ((month == 4 || month == 6 || month == 9 || month == 11) && day == 31)
                throw new ArgumentException($"Month {month} does not have 31 days.");

            _holidays.Add(new RecurringHoliday(month, day));
        }

        public void SetSingleHoliday(DateOnly date)
        {
            _holidays.Add(new SingleHoliday(date));
        }

        public void SetWorkdayStartAndEnd(TimeOnly start, TimeOnly end)
        {
            if (start >= end)
                throw new ArgumentException("Workday start must be earlier than end.");

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

            return forwardIncrement ? GetForwardEffectiveDateTime(date) : GetBackwardEffectiveDateTime(date);
        }

        private (DateOnly dateOnly, TimeOnly timeOnly) GetForwardEffectiveDateTime(DateTime date)
        {
            var effectiveDate = DateOnly.FromDateTime(date);
            var effectiveTime = TimeOnly.FromDateTime(date);

            if (!IsWorkday(effectiveDate))
            {
                effectiveDate = NextWorkday(effectiveDate);
                effectiveTime = _workdayStart;
            }
            else if (effectiveTime < _workdayStart)
            {
                effectiveTime = _workdayStart;
            }
            else if (effectiveTime > _workdayEnd)
            {
                effectiveTime = _workdayStart;
                effectiveDate = NextWorkday(effectiveDate);
            }

            return (effectiveDate, effectiveTime);
        }

        private (DateOnly dateOnly, TimeOnly timeOnly) GetBackwardEffectiveDateTime(DateTime date)
        {
            var effectiveDate = DateOnly.FromDateTime(date);
            var effectiveTime = TimeOnly.FromDateTime(date);

            if (!IsWorkday(effectiveDate))
            {
                effectiveDate = PreviousWorkday(effectiveDate);
                effectiveTime = _workdayEnd;
            }
            else if (effectiveTime < _workdayStart)
            {
                effectiveTime = _workdayEnd;
                effectiveDate = PreviousWorkday(effectiveDate);
            }
            else if (effectiveTime > _workdayEnd)
            {
                effectiveTime = _workdayEnd;
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
