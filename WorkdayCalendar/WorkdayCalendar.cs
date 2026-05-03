using WorkdayCalendar.Holidays;

namespace WorkdayCalendar
{
    public class WorkdayCalendar : IWorkdayCalendar
    {
        private readonly HashSet<IHoliday> _holidays = new();
        private WorkingHours _workingHours = new(new TimeOnly(8, 0), new TimeOnly(16, 0));

        public DateTime GetWorkdayIncrement(DateTime date, decimal workdays)
        {
            bool forward = workdays >= 0;
            var (effectiveDate, effectiveTime) = forward
                ? GetForwardEffectiveDateTime(date)
                : GetBackwardEffectiveDateTime(date);

            var offsetMinutes = forward
                ? _workingHours.MinutesFromStart(effectiveTime)
                : _workingHours.MinutesFromEnd(effectiveTime);

            var incrementMinutes = Math.Abs((double)workdays * _workingHours.TotalMinutes);
            var totalMinutes = offsetMinutes + incrementMinutes;

            int fullWorkdays = (int)Math.Floor(totalMinutes / _workingHours.TotalMinutes);
            var remainingMinutes = (int)Math.Floor(totalMinutes % _workingHours.TotalMinutes);

            var finalDate = MoveWorkingDays(effectiveDate, fullWorkdays, forward);

            var finalTime = forward
                ? _workingHours.Start.Add(TimeSpan.FromMinutes(remainingMinutes))
                : _workingHours.End.Add(-1 * TimeSpan.FromMinutes(remainingMinutes));

            return finalDate.ToDateTime(finalTime);
        }

        public void SetRecurringHoliday(int month, int day)
            => _holidays.Add(RecurringHoliday.Create(month, day));

        public void SetSingleHoliday(DateOnly date)
            => _holidays.Add(new SingleHoliday(date));

        public void SetWorkdayStartAndEnd(TimeOnly start, TimeOnly end)
            => _workingHours = new WorkingHours(start, end);

        private bool IsWorkday(DateOnly date) => !IsWeekend(date) && !IsHoliday(date);

        private static bool IsWeekend(DateOnly date) =>
            date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;

        private bool IsHoliday(DateOnly date) => _holidays.Any(h => h.IsHoliday(date));

        private (DateOnly date, TimeOnly time) GetForwardEffectiveDateTime(DateTime date)
        {
            var effectiveDate = DateOnly.FromDateTime(date);
            var effectiveTime = TimeOnly.FromDateTime(date);

            if (!IsWorkday(effectiveDate))
            {
                effectiveDate = NextWorkday(effectiveDate);
                effectiveTime = _workingHours.Start;
            }
            else if (effectiveTime < _workingHours.Start)
            {
                effectiveTime = _workingHours.Start;
            }
            else if (effectiveTime > _workingHours.End)
            {
                effectiveTime = _workingHours.Start;
                effectiveDate = NextWorkday(effectiveDate);
            }

            return (effectiveDate, effectiveTime);
        }

        private (DateOnly date, TimeOnly time) GetBackwardEffectiveDateTime(DateTime date)
        {
            var effectiveDate = DateOnly.FromDateTime(date);
            var effectiveTime = TimeOnly.FromDateTime(date);

            if (!IsWorkday(effectiveDate))
            {
                effectiveDate = PreviousWorkday(effectiveDate);
                effectiveTime = _workingHours.End;
            }
            else if (effectiveTime < _workingHours.Start)
            {
                effectiveTime = _workingHours.End;
                effectiveDate = PreviousWorkday(effectiveDate);
            }
            else if (effectiveTime > _workingHours.End)
            {
                effectiveTime = _workingHours.End;
            }

            return (effectiveDate, effectiveTime);
        }

        private DateOnly NextWorkday(DateOnly date)
        {
            var next = date.AddDays(1);
            while (!IsWorkday(next))
                next = next.AddDays(1);
            return next;
        }

        private DateOnly PreviousWorkday(DateOnly date)
        {
            var prev = date.AddDays(-1);
            while (!IsWorkday(prev))
                prev = prev.AddDays(-1);
            return prev;
        }

        private DateOnly MoveWorkingDays(DateOnly date, int days, bool moveForward = true)
        {
            var direction = moveForward ? 1 : -1;
            int remainingDays = days;
            var currentDate = date;

            while (remainingDays > 0) 
            {
                currentDate = currentDate.AddDays(direction);
                if (IsWorkday(currentDate))
                    remainingDays--;
            }

            return currentDate;
        }
    }
}
