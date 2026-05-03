using WorkdayCalendar.Holidays;

namespace WorkdayCalendar
{
    /// <summary>
    /// Provides functionality to calculate workdays, accounting for weekends and holidays.
    /// Supports both forward and backward calculations with fractional workday increments.
    /// </summary>
    public class WorkdayCalendar : IWorkdayCalendar
    {
        private readonly HashSet<IHoliday> _holidays = new();
        private WorkingHours _workingHours = new(new TimeOnly(8, 0), new TimeOnly(16, 0));

        /// <summary>
        /// Calculates a new datetime by adding or subtracting a specified number of working days from a given date.
        /// </summary>
        /// <param name="date">The starting datetime.</param>
        /// <param name="workdays">The number of workdays to add (positive) or subtract (negative). Fractional workdays are supported.</param>
        /// <returns>
        /// A datetime that results from moving forward or backward by the specified number of workdays.
        /// The resulting date is guaranteed to be a workday, and the time falls within working hours.
        /// </returns>
        public DateTime GetWorkdayIncrement(DateTime date, decimal workdays)
        {
            bool forward = workdays >= 0;

            // Normalize the input datetime to a valid working time position
            var (effectiveDate, effectiveTime) = forward
                ? GetForwardEffectiveDateTime(date)
                : GetBackwardEffectiveDateTime(date);

            // Calculate the offset in minutes from the normalized start point
            var offsetMinutes = forward
                ? _workingHours.MinutesFromStart(effectiveTime)
                : _workingHours.MinutesFromEnd(effectiveTime);

            // Convert workdays to minutes
            var incrementMinutes = Math.Abs((double)workdays * _workingHours.TotalMinutes);
            var totalMinutes = offsetMinutes + incrementMinutes;

            // Calculate full workdays and remaining minutes for fine-grained positioning
            int fullWorkdays = (int)Math.Floor(totalMinutes / _workingHours.TotalMinutes);
            var remainingMinutes = (int)Math.Floor(totalMinutes % _workingHours.TotalMinutes);

            // Move through the calendar by the calculated number of full workdays
            var finalDate = MoveWorkingDays(effectiveDate, fullWorkdays, forward);

            // Calculate the final time within the workday
            var finalTime = forward
                ? _workingHours.Start.Add(TimeSpan.FromMinutes(remainingMinutes))
                : _workingHours.End.Add(-1 * TimeSpan.FromMinutes(remainingMinutes));

            return finalDate.ToDateTime(finalTime);
        }

        /// <summary>
        /// Registers a recurring holiday that occurs on the same date every year.
        /// </summary>
        /// <param name="month">The month (1-12) of the recurring holiday.</param>
        /// <param name="day">The day (1-31) of the recurring holiday.</param>
        public void SetRecurringHoliday(int month, int day)
            => _holidays.Add(RecurringHoliday.Create(month, day));

        /// <summary>
        /// Registers a single-instance holiday on a specific date.
        /// </summary>
        /// <param name="date">The date to mark as a holiday.</param>
        public void SetSingleHoliday(DateOnly date)
            => _holidays.Add(new SingleHoliday(date));

        /// <summary>
        /// Defines the working hours for a business day.
        /// </summary>
        /// <param name="start">The start time of the working day.</param>
        /// <param name="end">The end time of the working day.</param>
        public void SetWorkdayStartAndEnd(TimeOnly start, TimeOnly end)
            => _workingHours = new WorkingHours(start, end);

        /// <summary>
        /// Determines whether the specified date is a workday (not a weekend and not a holiday).
        /// </summary>
        private bool IsWorkday(DateOnly date) => !IsWeekend(date) && !IsHoliday(date);

        /// <summary>
        /// Checks if the specified date falls on a weekend (Saturday or Sunday).
        /// </summary>
        private static bool IsWeekend(DateOnly date) =>
            date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;

        /// <summary>
        /// Checks if the specified date is registered as a holiday.
        /// </summary>
        private bool IsHoliday(DateOnly date) => _holidays.Any(h => h.IsHoliday(date));

        /// <summary>
        /// Normalizes the input datetime for forward calculations.
        /// Ensures the effective datetime falls within working hours on a workday.
        /// </summary>
        /// <remarks>
        /// - If the date is not a workday, moves to the next workday at start time.
        /// - If the time is before working hours, adjusts to start time.
        /// - If the time is after working hours, moves to next workday's start time.
        /// </remarks>
        private (DateOnly date, TimeOnly time) GetForwardEffectiveDateTime(DateTime date)
        {
            var effectiveDate = DateOnly.FromDateTime(date);
            var effectiveTime = TimeOnly.FromDateTime(date);

            if (!IsWorkday(effectiveDate))
            {
                // Not a workday, move to next workday at start time
                effectiveDate = NextWorkday(effectiveDate);
                effectiveTime = _workingHours.Start;
            }
            else if (effectiveTime < _workingHours.Start)
            {
                // Before working hours, adjust to start time
                effectiveTime = _workingHours.Start;
            }
            else if (effectiveTime > _workingHours.End)
            {
                // After working hours, move to next workday's start time
                effectiveTime = _workingHours.Start;
                effectiveDate = NextWorkday(effectiveDate);
            }

            return (effectiveDate, effectiveTime);
        }

        /// <summary>
        /// Normalizes the input datetime for backward calculations.
        /// Ensures the effective datetime falls within working hours on a workday.
        /// </summary>
        /// <remarks>
        /// - If the date is not a workday, moves to the previous workday at end time.
        /// - If the time is before working hours, adjusts to previous workday's end time.
        /// - If the time is after working hours, adjusts to end time.
        /// </remarks>
        private (DateOnly date, TimeOnly time) GetBackwardEffectiveDateTime(DateTime date)
        {
            var effectiveDate = DateOnly.FromDateTime(date);
            var effectiveTime = TimeOnly.FromDateTime(date);

            if (!IsWorkday(effectiveDate))
            {
                // Not a workday, move to previous workday at end time
                effectiveDate = PreviousWorkday(effectiveDate);
                effectiveTime = _workingHours.End;
            }
            else if (effectiveTime < _workingHours.Start)
            {
                // Before working hours, move to previous workday's end time
                effectiveTime = _workingHours.End;
                effectiveDate = PreviousWorkday(effectiveDate);
            }
            else if (effectiveTime > _workingHours.End)
            {
                // After working hours, adjust to end time
                effectiveTime = _workingHours.End;
            }

            return (effectiveDate, effectiveTime);
        }

        /// <summary>
        /// Finds the next workday starting from the given date.
        /// Skips weekends and holidays.
        /// </summary>
        private DateOnly NextWorkday(DateOnly date)
        {
            var next = date.AddDays(1);
            while (!IsWorkday(next))
                next = next.AddDays(1);
            return next;
        }

        /// <summary>
        /// Finds the previous workday starting from the given date.
        /// Skips weekends and holidays.
        /// </summary>
        private DateOnly PreviousWorkday(DateOnly date)
        {
            var prev = date.AddDays(-1);
            while (!IsWorkday(prev))
                prev = prev.AddDays(-1);
            return prev;
        }

        /// <summary>
        /// Moves forward or backward through the calendar by the specified number of full workdays.
        /// Skips weekends and holidays.
        /// </summary>
        /// <param name="date">The starting date.</param>
        /// <param name="days">The number of full workdays to move.</param>
        /// <param name="moveForward">If true, moves forward; if false, moves backward.</param>
        /// <returns>The date after moving the specified number of workdays.</returns>
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
