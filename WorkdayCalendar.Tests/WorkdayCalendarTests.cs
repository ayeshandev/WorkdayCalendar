namespace WorkdayCalendar.Tests
{
    public class WorkdayCalendarTests
    {
        private static WorkdayCalendar CreateCalendar()
        {
            var calendar = new WorkdayCalendar();
            calendar.SetWorkdayStartAndEnd(new TimeOnly(8, 0), new TimeOnly(16, 0));
            calendar.SetRecurringHoliday(5, 17);
            calendar.SetSingleHoliday(new DateOnly(2004, 5, 27));
            return calendar;
        }

        private static DateTime TruncateToMinute(DateTime dt) => new(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0);

        public class SpecExamples
        {
            private readonly WorkdayCalendar _calendar = CreateCalendar();

            [Fact]
            public void LargePositiveIncrement_SkipsSingleHoliday()
            {
                // 24-05-2004 19:03 + 44.723656 workdays = 27-07-2004 13:47
                var result = _calendar.GetWorkdayIncrement(new DateTime(2004, 5, 24, 19, 3, 0), 44.723656m);

                Assert.Equal(new DateTime(2004, 7, 27, 13, 47, 0), TruncateToMinute(result));
            }

            [Fact]
            public void PositiveIncrement_StartJustAfterWorkdayStart()
            {
                // 24-05-2004 08:03 + 12.782709 workdays = 10-06-2004 14:18
                var result = _calendar.GetWorkdayIncrement(new DateTime(2004, 5, 24, 8, 3, 0), 12.782709m);

                Assert.Equal(new DateTime(2004, 6, 10, 14, 18, 0), TruncateToMinute(result));
            }

            [Fact]
            public void PositiveIncrement_StartBeforeWorkdayStart()
            {
                // 24-05-2004 07:03 + 8.276628 workdays = 04-06-2004 10:12
                var result = _calendar.GetWorkdayIncrement(new DateTime(2004, 5, 24, 7, 3, 0), 8.276628m);

                Assert.Equal(new DateTime(2004, 6, 4, 10, 12, 0), TruncateToMinute(result));
            }

            [Fact]
            public void NegativeIncrement_SkipsRecurringHoliday()
            {
                // 24-05-2004 18:05 + (−5.5) workdays = 14-05-2004 12:00
                var result = _calendar.GetWorkdayIncrement(new DateTime(2004, 5, 24, 18, 5, 0), -5.5m);

                Assert.Equal(new DateTime(2004, 5, 14, 12, 0, 0), TruncateToMinute(result));
            }

            [Fact]
            public void NegativeIncrement_StartJustAfterWorkdayEnd()
            {
                // 24-05-2004 18:03 + (-6.7470217) workdays = 13-05-2004 10:02
                var result = _calendar.GetWorkdayIncrement(new DateTime(2004, 5, 24, 18, 3, 0), -6.7470217m);

                Assert.Equal(new DateTime(2004, 5, 13, 10, 2, 0), TruncateToMinute(result));
            }
        }
    }
}
