using WorkdayCalendar.Holidays;

namespace WorkdayCalendar.Tests;

public class SingleHolidayTests
{
    [Fact]
    public void IsHoliday_MatchingDate_ReturnsTrue()
    {
        var holiday = new SingleHoliday(new DateOnly(2004, 5, 27));

        Assert.True(holiday.IsHoliday(new DateOnly(2004, 5, 27)));
    }

    [Fact]
    public void IsHoliday_DifferentDate_ReturnsFalse()
    {
        var holiday = new SingleHoliday(new DateOnly(2004, 5, 27));

        Assert.False(holiday.IsHoliday(new DateOnly(2004, 5, 28)));
    }

    [Fact]
    public void IsHoliday_SameMonthDay_DifferentYear_ReturnsFalse()
    {
        var holiday = new SingleHoliday(new DateOnly(2004, 5, 27));

        Assert.False(holiday.IsHoliday(new DateOnly(2005, 5, 27)));
    }
}
