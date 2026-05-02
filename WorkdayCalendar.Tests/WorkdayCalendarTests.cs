namespace WorkdayCalendar.Tests;

/// <summary>
/// Tests for WorkdayCalendar.
///
/// Shared holiday setup (unless a test class specifies otherwise):
///   - 17 May: recurring holiday
///   - 27 May 2004: single holiday
///   - Workday: 08:00–16:00
/// </summary>
public class WorkdayCalendarTests
{
    // ── Shared factory ────────────────────────────────────────────────────────

    private static WorkdayCalendar CreateCalendar()
    {
        var cal = new WorkdayCalendar();
        cal.SetWorkdayStartAndEnd(new TimeOnly(8, 0), new TimeOnly(16, 0));
        cal.SetRecurringHoliday(5, 17);
        cal.SetSingleHoliday(new DateOnly(2004, 5, 27));
        return cal;
    }

    /// <summary>
    /// Truncate to the minute — floating-point arithmetic produces sub-second
    /// drift on very large increments, which is acceptable in a business context.
    /// </summary>
    private static DateTime TruncateToMinute(DateTime dt) =>
        new(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0);

    // ── Spec examples ─────────────────────────────────────────────────────────

    public class SpecExamples
    {
        private readonly WorkdayCalendar _cal = CreateCalendar();

        [Fact]
        public void NegativeIncrement_SkipsRecurringHoliday_MainExample()
        {
            // 24-05-2004 18:05 + (−5.5) workdays = 14-05-2004 12:00
            var result = _cal.GetWorkdayIncrement(new DateTime(2004, 5, 24, 18, 5, 0), -5.5m);

            Assert.Equal(new DateTime(2004, 5, 14, 12, 0, 0), TruncateToMinute(result));
        }

        [Fact]
        public void LargePositiveIncrement_SkipsSingleHoliday()
        {
            // 24-05-2004 19:03 + 44.723656 workdays = 27-07-2004 13:47
            var result = _cal.GetWorkdayIncrement(new DateTime(2004, 5, 24, 19, 3, 0), 44.723656m);

            Assert.Equal(new DateTime(2004, 7, 27, 13, 47, 0), TruncateToMinute(result));
        }

        [Fact]
        public void PositiveIncrement_StartJustAfterWorkdayStart()
        {
            // 24-05-2004 08:03 + 12.782709 workdays = 10-06-2004 14:18
            var result = _cal.GetWorkdayIncrement(new DateTime(2004, 5, 24, 8, 3, 0), 12.782709m);

            Assert.Equal(new DateTime(2004, 6, 10, 14, 18, 0), TruncateToMinute(result));
        }

        [Fact]
        public void PositiveIncrement_StartBeforeWorkdayStart()
        {
            // 24-05-2004 07:03 + 8.276628 workdays = 04-06-2004 10:12
            var result = _cal.GetWorkdayIncrement(new DateTime(2004, 5, 24, 7, 3, 0), 8.276628m);

            Assert.Equal(new DateTime(2004, 6, 4, 10, 12, 0), TruncateToMinute(result));
        }
    }

    // ── Zero increment ────────────────────────────────────────────────────────

    public class ZeroIncrement
    {
        private readonly WorkdayCalendar _cal = CreateCalendar();

        [Fact]
        public void ZeroIncrement_ReturnsUnchangedDateTime()
        {
            var start = new DateTime(2004, 5, 24, 10, 0, 0);
            Assert.Equal(start, _cal.GetWorkdayIncrement(start, 0m));
        }
    }

    // ── Workday boundary normalization ────────────────────────────────────────

    public class WorkdayBoundaries
    {
        private readonly WorkdayCalendar _cal = CreateCalendar();

        [Fact]
        public void Spec_QuarterDayFromPastEnd_OverflowsToNextDay()
        {
            // Per spec: 15:07 + 0.25 wd (2 h on 8 h day) = 09:07 next workday
            var result = _cal.GetWorkdayIncrement(new DateTime(2004, 5, 24, 15, 7, 0), 0.25m);

            Assert.Equal(new DateTime(2004, 5, 25, 9, 7, 0), TruncateToMinute(result));
        }

        [Fact]
        public void Spec_HalfDayFromBeforeStart_NormalizesToWorkdayStart()
        {
            // Per spec: 04:00 + 0.5 wd = 12:00 (same day, start snapped to 08:00)
            var result = _cal.GetWorkdayIncrement(new DateTime(2004, 5, 24, 4, 0, 0), 0.5m);

            Assert.Equal(new DateTime(2004, 5, 24, 12, 0, 0), TruncateToMinute(result));
        }

        [Fact]
        public void Forward_TimeAtWorkdayEnd_StartsNextWorkday()
        {
            // 16:00 is AT the end — treated as "past end" for forward direction
            var result = _cal.GetWorkdayIncrement(new DateTime(2004, 5, 24, 16, 0, 0), 0.25m);

            Assert.Equal(new DateTime(2004, 5, 25, 10, 0, 0), TruncateToMinute(result));
        }

        [Fact]
        public void Forward_TimeAtWorkdayStart_CountsFromStart()
        {
            // 08:00 + 0.25 wd (2 h) = 10:00 same day
            var result = _cal.GetWorkdayIncrement(new DateTime(2004, 5, 24, 8, 0, 0), 0.25m);

            Assert.Equal(new DateTime(2004, 5, 24, 10, 0, 0), TruncateToMinute(result));
        }

        [Fact]
        public void Backward_TimeAfterWorkdayEnd_SnapsToEnd()
        {
            // 18:05 → effective 16:00; subtract 0.5 wd (4 h) = 12:00 same day
            var result = _cal.GetWorkdayIncrement(new DateTime(2004, 5, 24, 18, 5, 0), -0.5m);

            Assert.Equal(new DateTime(2004, 5, 24, 12, 0, 0), TruncateToMinute(result));
        }

        [Fact]
        public void Backward_TimeAtWorkdayEnd_SubtractsFromEnd()
        {
            // 16:00 is within range for backward (not strictly past end)
            // 16:00 → offset 480; subtract 0.5 wd (4 h) = 12:00 same day
            var result = _cal.GetWorkdayIncrement(new DateTime(2004, 5, 24, 16, 0, 0), -0.5m);

            Assert.Equal(new DateTime(2004, 5, 24, 12, 0, 0), TruncateToMinute(result));
        }

        [Fact]
        public void Backward_TimeAtWorkdayStart_UsesPreviousWorkdayEnd()
        {
            // 08:00 on Monday → previous working day (Friday 21 May) at 16:00
            // subtract 0.25 wd (2 h) = 14:00 Friday
            var result = _cal.GetWorkdayIncrement(new DateTime(2004, 5, 24, 8, 0, 0), -0.25m);

            Assert.Equal(new DateTime(2004, 5, 21, 14, 0, 0), TruncateToMinute(result));
        }

        [Fact]
        public void Backward_TimeBeforeWorkdayStart_UsesPreviousWorkdayEnd()
        {
            // 07:00 on Monday → same as 08:00 case above
            var result = _cal.GetWorkdayIncrement(new DateTime(2004, 5, 24, 7, 0, 0), -0.25m);

            Assert.Equal(new DateTime(2004, 5, 21, 14, 0, 0), TruncateToMinute(result));
        }
    }

    // ── Weekend handling ──────────────────────────────────────────────────────

    public class WeekendHandling
    {
        private readonly WorkdayCalendar _cal = CreateCalendar();

        [Fact]
        public void Forward_StartOnSaturday_NormalizesToNextMonday()
        {
            // Saturday 22 May + 0.5 wd → Monday 24 May at 12:00
            var result = _cal.GetWorkdayIncrement(new DateTime(2004, 5, 22, 9, 0, 0), 0.5m);

            Assert.Equal(new DateTime(2004, 5, 24, 12, 0, 0), TruncateToMinute(result));
        }

        [Fact]
        public void Backward_StartOnSunday_NormalizesToPreviousFriday()
        {
            // Sunday 23 May going backward → Friday 21 May at 16:00
            // subtract 0.25 wd (2 h) = 14:00 Friday
            var result = _cal.GetWorkdayIncrement(new DateTime(2004, 5, 23, 9, 0, 0), -0.25m);

            Assert.Equal(new DateTime(2004, 5, 21, 14, 0, 0), TruncateToMinute(result));
        }

        [Fact]
        public void Forward_ResultOverflowsFridayIntoMonday()
        {
            // Friday 21 May 14:00 + 0.5 wd (4 h): 14:00 + 4h = 18:00 → overflows to Monday 10:00
            var result = _cal.GetWorkdayIncrement(new DateTime(2004, 5, 21, 14, 0, 0), 0.5m);

            Assert.Equal(new DateTime(2004, 5, 24, 10, 0, 0), TruncateToMinute(result));
        }
    }

    // ── Holiday handling ──────────────────────────────────────────────────────

    public class HolidayHandling
    {
        private readonly WorkdayCalendar _cal = CreateCalendar();

        [Fact]
        public void Forward_SkipsRecurringHoliday_May17()
        {
            // Friday 14 May + 1 wd → should skip May 17 (holiday) → Tuesday 18 May
            var result = _cal.GetWorkdayIncrement(new DateTime(2004, 5, 14, 8, 0, 0), 1m);

            Assert.Equal(new DateTime(2004, 5, 18, 8, 0, 0), TruncateToMinute(result));
        }

        [Fact]
        public void Forward_SkipsSingleHoliday_May27()
        {
            // Wednesday 26 May + 1 wd → skip May 27 (single holiday) → Friday 28 May
            var result = _cal.GetWorkdayIncrement(new DateTime(2004, 5, 26, 8, 0, 0), 1m);

            Assert.Equal(new DateTime(2004, 5, 28, 8, 0, 0), TruncateToMinute(result));
        }

        [Fact]
        public void Forward_StartOnHoliday_NormalizesToNextWorkday()
        {
            // May 17 (holiday, Monday) at 10:00 → normalize to May 18 at 08:00
            // + 1 wd → May 19 08:00
            var result = _cal.GetWorkdayIncrement(new DateTime(2004, 5, 17, 10, 0, 0), 1m);

            Assert.Equal(new DateTime(2004, 5, 19, 8, 0, 0), TruncateToMinute(result));
        }

        [Fact]
        public void Backward_StartOnHoliday_NormalizesToPreviousWorkday()
        {
            // May 17 (holiday, Monday) at 10:00 backward → normalize to May 14 (Friday) at 16:00
            // subtract 1 wd → 480 min offset + (−480) = 0 → May 14 08:00
            var result = _cal.GetWorkdayIncrement(new DateTime(2004, 5, 17, 10, 0, 0), -1m);

            Assert.Equal(new DateTime(2004, 5, 14, 8, 0, 0), TruncateToMinute(result));
        }

        [Fact]
        public void RecurringHoliday_AppliesToDifferentYears()
        {
            // May 17 is skipped in 2005 too
            var result = _cal.GetWorkdayIncrement(new DateTime(2005, 5, 16, 8, 0, 0), 1m);

            Assert.Equal(new DateTime(2005, 5, 18, 8, 0, 0), TruncateToMinute(result));
        }
    }

    // ── Fractional day accuracy ───────────────────────────────────────────────

    public class FractionalDays
    {
        private readonly WorkdayCalendar _cal = CreateCalendar();

        [Fact]
        public void QuarterDay_Adds2Hours()
        {
            var result = _cal.GetWorkdayIncrement(new DateTime(2004, 5, 24, 10, 0, 0), 0.25m);

            Assert.Equal(new DateTime(2004, 5, 24, 12, 0, 0), TruncateToMinute(result));
        }

        [Fact]
        public void HalfDay_Adds4Hours()
        {
            var result = _cal.GetWorkdayIncrement(new DateTime(2004, 5, 24, 8, 0, 0), 0.5m);

            Assert.Equal(new DateTime(2004, 5, 24, 12, 0, 0), TruncateToMinute(result));
        }

        [Fact]
        public void OneFullDay_AdvancesOneWorkday()
        {
            var result = _cal.GetWorkdayIncrement(new DateTime(2004, 5, 24, 8, 0, 0), 1m);

            Assert.Equal(new DateTime(2004, 5, 25, 8, 0, 0), TruncateToMinute(result));
        }

        [Fact]
        public void NegativeHalfDay_Subtracts4Hours()
        {
            // 16:00 − 0.5 wd (4 h) = 12:00 same day (16:00 is in-range for backward)
            var result = _cal.GetWorkdayIncrement(new DateTime(2004, 5, 24, 16, 0, 0), -0.5m);

            Assert.Equal(new DateTime(2004, 5, 24, 12, 0, 0), TruncateToMinute(result));
        }
    }

    // ── Custom working hours ──────────────────────────────────────────────────

    public class CustomWorkingHours
    {
        [Fact]
        public void CustomHours_09To17_CalculatesCorrectly()
        {
            var cal = new WorkdayCalendar();
            cal.SetWorkdayStartAndEnd(new TimeOnly(9, 0), new TimeOnly(17, 0));

            // 08:00 + 0.5 wd (4 h on an 8 h day) = 13:00
            var result = cal.GetWorkdayIncrement(new DateTime(2024, 1, 2, 9, 0, 0), 0.5m);

            Assert.Equal(new DateTime(2024, 1, 2, 13, 0, 0), TruncateToMinute(result));
        }

        [Fact]
        public void SetWorkdayStartAndStop_InvalidRange_Throws()
        {
            var cal = new WorkdayCalendar();

            Assert.Throws<ArgumentException>(() =>
                cal.SetWorkdayStartAndEnd(new TimeOnly(16, 0), new TimeOnly(8, 0)));
        }

        [Fact]
        public void SetWorkdayStartAndStop_EqualTimes_Throws()
        {
            var cal = new WorkdayCalendar();

            Assert.Throws<ArgumentException>(() =>
                cal.SetWorkdayStartAndEnd(new TimeOnly(9, 0), new TimeOnly(9, 0)));
        }
    }
}
