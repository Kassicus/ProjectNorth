using ProjectNorth.Sim.Calendar;

namespace ProjectNorth.Sim.Tests.Calendar;

public class GameDateTests
{
    [Fact]
    public void CalendarShape_MatchesTheDecidedSpine()
    {
        // GDD §3, decided v0.3 — Stardew-style. A year is 112 days / 16 weeks / 16 plane visits.
        Assert.Equal(28, GameDate.DaysPerSeason);
        Assert.Equal(4, GameDate.SeasonsPerYear);
        Assert.Equal(112, GameDate.DaysPerYear);
        Assert.Equal(7, GameDate.DaysPerWeek);
    }

    [Fact]
    public void DayZero_IsYearOneSpringDayOne()
    {
        var date = GameDate.FromTotalDays(0);

        Assert.Equal(1, date.Year);
        Assert.Equal(Season.Spring, date.CalendarSeason);
        Assert.Equal(1, date.DayOfSeason);
    }

    [Fact]
    public void LastDayOfSpring_IsDayTwentySeven()
    {
        var date = GameDate.FromTotalDays(27);

        Assert.Equal(1, date.Year);
        Assert.Equal(Season.Spring, date.CalendarSeason);
        Assert.Equal(28, date.DayOfSeason);
    }

    [Fact]
    public void DayTwentyEight_RollsIntoSummer()
    {
        var date = GameDate.FromTotalDays(28);

        Assert.Equal(1, date.Year);
        Assert.Equal(Season.Summer, date.CalendarSeason);
        Assert.Equal(1, date.DayOfSeason);
    }

    [Fact]
    public void DayOneHundredTwelve_IsYearTwoSpringDayOne()
    {
        var date = GameDate.FromTotalDays(112);

        Assert.Equal(2, date.Year);
        Assert.Equal(Season.Spring, date.CalendarSeason);
        Assert.Equal(1, date.DayOfSeason);
    }

    [Theory]
    [InlineData(0, 1, Season.Spring, 1)]
    [InlineData(55, 1, Season.Summer, 28)]
    [InlineData(56, 1, Season.Autumn, 1)]
    [InlineData(83, 1, Season.Autumn, 28)]
    [InlineData(84, 1, Season.Winter, 1)]
    [InlineData(111, 1, Season.Winter, 28)]
    [InlineData(224, 3, Season.Spring, 1)]
    public void FromTotalDays_MapsSeasonBoundaries(
        int totalDays, int expectedYear, Season expectedSeason, int expectedDay)
    {
        var date = GameDate.FromTotalDays(totalDays);

        Assert.Equal(expectedYear, date.Year);
        Assert.Equal(expectedSeason, date.CalendarSeason);
        Assert.Equal(expectedDay, date.DayOfSeason);
    }

    [Fact]
    public void TotalDays_RoundTripsAcrossMoreThanThreeYears()
    {
        for (var day = 0; day < GameDate.DaysPerYear * 3.5; day++)
        {
            Assert.Equal(day, GameDate.FromTotalDays(day).TotalDays);
        }
    }

    [Fact]
    public void FromTotalDays_RejectsNegativeDays()
    {
        // Day 0 is the epoch — arrival. There is no "before the game began".
        Assert.Throws<ArgumentOutOfRangeException>(() => GameDate.FromTotalDays(-1));
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(7, 1)]
    [InlineData(8, 2)]
    [InlineData(14, 2)]
    [InlineData(15, 3)]
    [InlineData(21, 3)]
    [InlineData(22, 4)]
    [InlineData(28, 4)]
    public void WeekOfSeason_RunsOneToFour(int dayOfSeason, int expectedWeek)
    {
        Assert.Equal(expectedWeek, new GameDate(1, Season.Spring, dayOfSeason).WeekOfSeason);
    }

    [Fact]
    public void IsWeekBoundary_MarksThePlanesCadence()
    {
        // GDD §4 — the plane comes on the week boundary. 16 visits a year.
        var boundaries = Enumerable.Range(0, GameDate.DaysPerYear)
            .Where(d => GameDate.FromTotalDays(d).IsWeekBoundary)
            .ToList();

        Assert.Equal(16, boundaries.Count);
        Assert.All(boundaries, d => Assert.Equal(0, GameDate.FromTotalDays(d).DayOfSeason % 7));
    }

    [Theory]
    [InlineData(6, false)]
    [InlineData(7, true)]
    [InlineData(8, false)]
    [InlineData(14, true)]
    [InlineData(21, true)]
    [InlineData(27, false)]
    [InlineData(28, true)]
    public void IsWeekBoundary_IsExactlyDaySevenFourteenTwentyOneTwentyEight(int dayOfSeason, bool expected)
    {
        Assert.Equal(expected, new GameDate(1, Season.Spring, dayOfSeason).IsWeekBoundary);
    }

    [Fact]
    public void ToString_IsReadable()
    {
        Assert.Equal("Y1 Spring 12", new GameDate(1, Season.Spring, 12).ToString());
        Assert.Equal("Y2 Winter 28", new GameDate(2, Season.Winter, 28).ToString());
    }

    [Theory]
    [InlineData(0, Season.Spring, 1)]
    [InlineData(1, Season.Spring, 0)]
    [InlineData(1, Season.Spring, 29)]
    public void Constructor_RejectsImpossibleDates(int year, Season season, int dayOfSeason)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GameDate(year, season, dayOfSeason));
    }

    [Fact]
    public void ValueEquality_ComesFromRecordStruct()
    {
        Assert.Equal(new GameDate(2, Season.Autumn, 3), new GameDate(2, Season.Autumn, 3));
        Assert.NotEqual(new GameDate(2, Season.Autumn, 3), new GameDate(2, Season.Autumn, 4));
    }

    [Fact]
    public void SeasonEnum_OrdersTheYearCorrectly()
    {
        // FromTotalDays casts the season slot straight to this enum, so the order is load-bearing.
        Assert.Equal(0, (int)Season.Spring);
        Assert.Equal(1, (int)Season.Summer);
        Assert.Equal(2, (int)Season.Autumn);
        Assert.Equal(3, (int)Season.Winter);
    }
}
