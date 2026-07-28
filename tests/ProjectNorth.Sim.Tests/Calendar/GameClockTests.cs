using ProjectNorth.Sim.Calendar;

namespace ProjectNorth.Sim.Tests.Calendar;

public class GameClockTests
{
    /// <summary>Records every boundary event a clock fires, in order, for assertion.</summary>
    private sealed class EventLog
    {
        private readonly List<(string Event, GameDate Date)> _entries = [];

        public EventLog(GameClock clock)
        {
            clock.DayEnded += d => _entries.Add(("DayEnded", d));
            clock.WeekEnded += d => _entries.Add(("WeekEnded", d));
            clock.CalendarSeasonChanged += d => _entries.Add(("CalendarSeasonChanged", d));
            clock.YearEnded += d => _entries.Add(("YearEnded", d));
        }

        public IReadOnlyList<(string Event, GameDate Date)> Entries => _entries;

        public int Count(string eventName) => _entries.Count(e => e.Event == eventName);

        public IEnumerable<string> Names => _entries.Select(e => e.Event);
    }

    [Fact]
    public void MinutesPerDay_IsTwentyFourHours()
    {
        Assert.Equal(1440, GameClock.MinutesPerDay);
    }

    [Fact]
    public void NewClock_StartsAtTheEpoch()
    {
        var clock = new GameClock();

        Assert.Equal(0, clock.TotalMinutes);
        Assert.Equal(0, clock.MinuteOfDay);
        Assert.Equal(0, clock.TotalDays);
        Assert.Equal(new GameDate(1, Season.Spring, 1), clock.CurrentDate);
    }

    [Fact]
    public void Constructor_RestoresFromTotalMinutes()
    {
        var clock = new GameClock((28 * GameClock.MinutesPerDay) + 360);

        Assert.Equal(28, clock.TotalDays);
        Assert.Equal(360, clock.MinuteOfDay);
        Assert.Equal(new GameDate(1, Season.Summer, 1), clock.CurrentDate);
    }

    [Fact]
    public void Constructor_RejectsNegativeMinutes()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GameClock(-1));
    }

    [Fact]
    public void Advance_RejectsNegativeMinutes()
    {
        var clock = new GameClock();
        Assert.Throws<ArgumentOutOfRangeException>(() => clock.Advance(-1));
    }

    [Fact]
    public void Advance_ByZero_IsANoOp()
    {
        var clock = new GameClock();
        var log = new EventLog(clock);

        clock.Advance(0);

        Assert.Equal(0, clock.TotalMinutes);
        Assert.Empty(log.Entries);
    }

    [Fact]
    public void OneDay_FiresDayEndedExactlyOnceAndLandsAtMidnight()
    {
        var clock = new GameClock();
        var log = new EventLog(clock);

        clock.Advance(GameClock.MinutesPerDay);

        Assert.Equal(1, log.Count("DayEnded"));
        Assert.Equal(0, clock.MinuteOfDay);
        Assert.Equal(1, clock.TotalDays);
        Assert.Equal(new GameDate(1, Season.Spring, 1), log.Entries[0].Date);
    }

    [Fact]
    public void SevenDays_FiresWeekEndedOnce()
    {
        var clock = new GameClock();
        var log = new EventLog(clock);

        clock.Advance(7 * GameClock.MinutesPerDay);

        Assert.Equal(7, log.Count("DayEnded"));
        Assert.Equal(1, log.Count("WeekEnded"));

        // The plane comes on day 7 — the event carries the day that ended, not the new one.
        var weekEnded = log.Entries.Single(e => e.Event == "WeekEnded").Date;
        Assert.Equal(new GameDate(1, Season.Spring, 7), weekEnded);
    }

    /// <summary>
    /// The load-bearing contract (brief §1d): one large Advance must fire every boundary it
    /// crosses, once each, in chronological order — not collapse them into a single jump.
    /// A season of sim time processed in one call while the player slept still owes the
    /// event layer four plane days.
    /// </summary>
    [Fact]
    public void SingleTwentyEightDayAdvance_FiresEveryCrossedBoundaryOnce()
    {
        var clock = new GameClock();
        var log = new EventLog(clock);

        clock.Advance(28 * GameClock.MinutesPerDay);

        Assert.Equal(28, log.Count("DayEnded"));
        Assert.Equal(4, log.Count("WeekEnded"));
        Assert.Equal(1, log.Count("CalendarSeasonChanged"));
        Assert.Equal(0, log.Count("YearEnded"));
        Assert.Equal(Season.Summer, clock.CurrentDate.CalendarSeason);
        Assert.Equal(new GameDate(1, Season.Summer, 1), clock.CurrentDate);
    }

    [Fact]
    public void BoundaryEvents_ArriveInChronologicalOrder()
    {
        var clock = new GameClock();
        var log = new EventLog(clock);

        clock.Advance(28 * GameClock.MinutesPerDay);

        var dayEndedDates = log.Entries
            .Where(e => e.Event == "DayEnded")
            .Select(e => e.Date.TotalDays)
            .ToList();

        Assert.Equal(Enumerable.Range(0, 28), dayEndedDates);
    }

    [Fact]
    public void PerDayEvents_FireInTheFixedOrder()
    {
        // Day 28 of Spring is both a week boundary and the last day of the season, so the
        // full ordering — DayEnded, WeekEnded, CalendarSeasonChanged — is observable.
        var clock = new GameClock(27 * GameClock.MinutesPerDay);
        var log = new EventLog(clock);

        clock.Advance(GameClock.MinutesPerDay);

        Assert.Equal(["DayEnded", "WeekEnded", "CalendarSeasonChanged"], log.Names);
        Assert.Equal(new GameDate(1, Season.Spring, 28), log.Entries[0].Date);
        Assert.Equal(new GameDate(1, Season.Spring, 28), log.Entries[1].Date);

        // Season and year events carry the NEW date — the page the calendar just turned to.
        Assert.Equal(new GameDate(1, Season.Summer, 1), log.Entries[2].Date);
    }

    [Fact]
    public void YearRollover_FiresDayWeekSeasonAndYearInOrder()
    {
        var clock = new GameClock(111 * GameClock.MinutesPerDay);
        var log = new EventLog(clock);

        clock.Advance(GameClock.MinutesPerDay);

        Assert.Equal(["DayEnded", "WeekEnded", "CalendarSeasonChanged", "YearEnded"], log.Names);
        Assert.Equal(new GameDate(1, Season.Winter, 28), log.Entries[0].Date);
        Assert.Equal(new GameDate(2, Season.Spring, 1), log.Entries[3].Date);
    }

    [Fact]
    public void FullYearAdvance_FiresSixteenPlaneDaysAndOneYearEnd()
    {
        var clock = new GameClock();
        var log = new EventLog(clock);

        clock.Advance(GameDate.DaysPerYear * GameClock.MinutesPerDay);

        Assert.Equal(112, log.Count("DayEnded"));
        Assert.Equal(16, log.Count("WeekEnded"));
        Assert.Equal(4, log.Count("CalendarSeasonChanged"));
        Assert.Equal(1, log.Count("YearEnded"));
        Assert.Equal(new GameDate(2, Season.Spring, 1), clock.CurrentDate);
    }

    [Fact]
    public void OneMinuteShortOfMidnight_FiresNothing()
    {
        var clock = new GameClock();
        var log = new EventLog(clock);

        clock.Advance(GameClock.MinutesPerDay - 1);

        Assert.Empty(log.Entries);
        Assert.Equal(1439, clock.MinuteOfDay);
        Assert.Equal(0, clock.TotalDays);
    }

    [Fact]
    public void TheNextMinute_FiresTheRollover()
    {
        var clock = new GameClock();
        clock.Advance(GameClock.MinutesPerDay - 1);

        var log = new EventLog(clock);
        clock.Advance(1);

        Assert.Equal(1, log.Count("DayEnded"));
        Assert.Equal(0, clock.MinuteOfDay);
        Assert.Equal(new GameDate(1, Season.Spring, 2), clock.CurrentDate);
    }

    [Fact]
    public void ManySmallAdvances_MatchOneLargeAdvance()
    {
        // Presentation feeds the clock a few minutes per frame; the balance harness feeds it
        // a season at a time. Both must produce the same history.
        var incremental = new GameClock();
        var incrementalLog = new EventLog(incremental);
        for (var i = 0; i < 28 * GameClock.MinutesPerDay; i += 7)
        {
            incremental.Advance(7);
        }

        var bulk = new GameClock();
        var bulkLog = new EventLog(bulk);
        bulk.Advance(28 * GameClock.MinutesPerDay);

        Assert.Equal(bulk.TotalMinutes, incremental.TotalMinutes);
        Assert.Equal(bulkLog.Entries, incrementalLog.Entries);
    }
}
