using WorkdayCalendar.Holidays;

namespace WorkdayCalendar.Tests;

public class RecurringHolidayTests
{
    // ── Creation / validation ──────────────────────────────────────────────

    [Fact]
    public void Create_ValidMonthAndDay_ReturnsInstance()
    {
        var holiday = RecurringHoliday.Create(5, 17);

        Assert.NotNull(holiday);
        Assert.Equal(5, holiday.Month);
        Assert.Equal(17, holiday.Day);
    }

    [Fact]
    public void Create_MonthBelowRange_Throws()
        => Assert.Throws<ArgumentOutOfRangeException>(() => RecurringHoliday.Create(0, 1));

    [Fact]
    public void Create_MonthAboveRange_Throws()
        => Assert.Throws<ArgumentOutOfRangeException>(() => RecurringHoliday.Create(13, 1));

    [Fact]
    public void Create_DayBelowRange_Throws()
        => Assert.Throws<ArgumentOutOfRangeException>(() => RecurringHoliday.Create(1, 0));

    [Fact]
    public void Create_DayAboveRange_Throws()
        => Assert.Throws<ArgumentOutOfRangeException>(() => RecurringHoliday.Create(1, 32));

    [Fact]
    public void Create_February29_Throws()
        => Assert.Throws<ArgumentException>(() => RecurringHoliday.Create(2, 29));

    [Fact]
    public void Create_February30_Throws()
        => Assert.Throws<ArgumentException>(() => RecurringHoliday.Create(2, 30));

    [Theory]
    [InlineData(4)]   // April
    [InlineData(6)]   // June
    [InlineData(9)]   // September
    [InlineData(11)]  // November
    public void Create_30DayMonth_Day31_Throws(int month)
        => Assert.Throws<ArgumentException>(() => RecurringHoliday.Create(month, 31));

    // ── IsHoliday ──────────────────────────────────────────────────────────

    [Fact]
    public void IsHoliday_MatchingMonthAndDay_ReturnsTrue()
    {
        var holiday = RecurringHoliday.Create(5, 17);

        Assert.True(holiday.IsHoliday(new DateOnly(2004, 5, 17)));
    }

    [Fact]
    public void IsHoliday_DifferentMonth_ReturnsFalse()
    {
        var holiday = RecurringHoliday.Create(5, 17);

        Assert.False(holiday.IsHoliday(new DateOnly(2004, 6, 17)));
    }

    [Fact]
    public void IsHoliday_DifferentDay_ReturnsFalse()
    {
        var holiday = RecurringHoliday.Create(5, 17);

        Assert.False(holiday.IsHoliday(new DateOnly(2004, 5, 18)));
    }

    [Fact]
    public void IsHoliday_SameMonthDay_DifferentYear_ReturnsTrue()
    {
        var holiday = RecurringHoliday.Create(5, 17);

        Assert.True(holiday.IsHoliday(new DateOnly(2025, 5, 17)));
    }
}
