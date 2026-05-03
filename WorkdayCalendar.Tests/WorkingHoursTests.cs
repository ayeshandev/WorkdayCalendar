namespace WorkdayCalendar.Tests;

public class WorkingHoursTests
{
    // ── Constructor ────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_ValidRange_SetsProperties()
    {
        var wh = new WorkingHours(new TimeOnly(8, 0), new TimeOnly(16, 0));

        Assert.Equal(new TimeOnly(8, 0), wh.Start);
        Assert.Equal(new TimeOnly(16, 0), wh.End);
    }

    [Fact]
    public void Constructor_StartEqualToEnd_Throws()
        => Assert.Throws<ArgumentException>(() =>
            new WorkingHours(new TimeOnly(9, 0), new TimeOnly(9, 0)));

    [Fact]
    public void Constructor_StartAfterEnd_Throws()
        => Assert.Throws<ArgumentException>(() =>
            new WorkingHours(new TimeOnly(16, 0), new TimeOnly(8, 0)));

    // ── TotalMinutes ───────────────────────────────────────────────────────

    [Fact]
    public void TotalMinutes_ReturnsCorrectDuration()
    {
        var wh = new WorkingHours(new TimeOnly(8, 0), new TimeOnly(16, 0));

        Assert.Equal(480, wh.TotalMinutes);
    }

    // ── MinutesFromStart ───────────────────────────────────────────────────

    [Fact]
    public void MinutesFromStart_AtStart_ReturnsZero()
    {
        var wh = new WorkingHours(new TimeOnly(8, 0), new TimeOnly(16, 0));

        Assert.Equal(0, wh.MinutesFromStart(new TimeOnly(8, 0)));
    }

    [Fact]
    public void MinutesFromStart_AtEnd_ReturnsTotalMinutes()
    {
        var wh = new WorkingHours(new TimeOnly(8, 0), new TimeOnly(16, 0));

        Assert.Equal(480, wh.MinutesFromStart(new TimeOnly(16, 0)));
    }

    // ── MinutesFromEnd ─────────────────────────────────────────────────────

    [Fact]
    public void MinutesFromEnd_AtEnd_ReturnsZero()
    {
        var wh = new WorkingHours(new TimeOnly(8, 0), new TimeOnly(16, 0));

        Assert.Equal(0, wh.MinutesFromEnd(new TimeOnly(16, 0)));
    }

    [Fact]
    public void MinutesFromEnd_AtStart_ReturnsTotalMinutes()
    {
        var wh = new WorkingHours(new TimeOnly(8, 0), new TimeOnly(16, 0));

        Assert.Equal(480, wh.MinutesFromEnd(new TimeOnly(8, 0)));
    }
}
