using ProjectNorth.Sim.Calendar;

namespace ProjectNorth.Sim.Tests.Calendar;

/// <summary>
/// CLAUDE.md rule 3 in test form. The calendar and the world are two different things, and
/// the game's two biggest narrative beats — the Winter That Stays and the False Thaw — are
/// nothing more than them disagreeing on purpose (GDD §3, TECH §3).
/// </summary>
public class SeasonControllerTests
{
    private static (GameClock Clock, SeasonController Seasons) Build(long totalMinutes = 0)
    {
        var clock = new GameClock(totalMinutes);
        return (clock, new SeasonController(clock));
    }

    private static long DaysToMinutes(int days) => (long)days * GameClock.MinutesPerDay;

    private static List<(Season From, Season To)> Watch(SeasonController seasons)
    {
        var transitions = new List<(Season, Season)>();
        seasons.SeasonChanged += (from, to) => transitions.Add((from, to));
        return transitions;
    }

    [Fact]
    public void NewController_FollowsTheCalendar()
    {
        var (_, seasons) = Build();

        Assert.Equal(SeasonMode.FollowCalendar, seasons.Mode);
        Assert.Equal(Season.Spring, seasons.ActualSeason);
    }

    [Fact]
    public void NewController_AdoptsTheClocksCurrentSeason()
    {
        var (_, seasons) = Build(DaysToMinutes(84));

        Assert.Equal(Season.Winter, seasons.ActualSeason);
    }

    [Fact]
    public void InFollowCalendarMode_TheWorldTracksThePage()
    {
        var (clock, seasons) = Build();
        var transitions = Watch(seasons);

        clock.Advance((int)DaysToMinutes(28));

        Assert.Equal(Season.Summer, seasons.ActualSeason);
        Assert.Equal([(Season.Spring, Season.Summer)], transitions);
    }

    [Fact]
    public void InFollowCalendarMode_AFullYearWalksAllFourSeasons()
    {
        var (clock, seasons) = Build();
        var transitions = Watch(seasons);

        clock.Advance((int)DaysToMinutes(GameDate.DaysPerYear));

        Assert.Equal(Season.Spring, seasons.ActualSeason);
        Assert.Equal(
            [
                (Season.Spring, Season.Summer),
                (Season.Summer, Season.Autumn),
                (Season.Autumn, Season.Winter),
                (Season.Winter, Season.Spring),
            ],
            transitions);
    }

    /// <summary>
    /// <strong>The Winter That Stays.</strong> The calendar keeps flipping pages. The world
    /// does not follow. Two season slots go by and it is still winter outside.
    /// </summary>
    [Fact]
    public void WinterThatStays_CalendarAdvancesWhileTheWorldDoesNot()
    {
        // Stand at the start of Y1 Winter, then refuse to let go of it.
        var (clock, seasons) = Build(DaysToMinutes(84));
        Assert.Equal(Season.Winter, seasons.ActualSeason);

        seasons.Pin(Season.Winter);
        var transitions = Watch(seasons);

        clock.Advance((int)DaysToMinutes(2 * GameDate.DaysPerSeason));

        Assert.Equal(Season.Summer, clock.CurrentDate.CalendarSeason);
        Assert.Equal(Season.Winter, seasons.ActualSeason);
        Assert.Equal(SeasonMode.Pinned, seasons.Mode);

        // Nothing changed outside, so nothing was announced.
        Assert.Empty(transitions);
    }

    /// <summary>
    /// <strong>The False Thaw</strong> (GDD §3, Act 3). Spring begins on schedule — ice
    /// candles, meltwater, first buds — runs about ten days, then reverses into winter while
    /// the calendar still reads Spring. This is the whole Act 3 premise, and per TECH §3 it
    /// has to be expressible from the very first commit.
    /// </summary>
    [Fact]
    public void FalseThaw_SpringReversesIntoWinterWhileTheCalendarStillReadsSpring()
    {
        // Y2 Spring 1.
        var (clock, seasons) = Build(DaysToMinutes(GameDate.DaysPerYear));
        Assert.Equal(new GameDate(2, Season.Spring, 1), clock.CurrentDate);

        var transitions = Watch(seasons);

        seasons.Pin(Season.Spring);
        clock.Advance((int)DaysToMinutes(10));
        seasons.Pin(Season.Winter);

        Assert.Equal(Season.Winter, seasons.ActualSeason);

        // The reversal is observable — Presentation needs it to drive the world freezing over.
        Assert.Equal([(Season.Spring, Season.Winter)], transitions);

        // And the calendar is still cheerfully insisting it is spring.
        Assert.Equal(Season.Spring, clock.CurrentDate.CalendarSeason);
        Assert.Equal(new GameDate(2, Season.Spring, 11), clock.CurrentDate);
    }

    [Fact]
    public void Pin_ToTheSeasonAlreadyInEffect_ChangesModeButAnnouncesNothing()
    {
        var (_, seasons) = Build();
        var transitions = Watch(seasons);

        seasons.Pin(Season.Spring);

        Assert.Equal(SeasonMode.Pinned, seasons.Mode);
        Assert.Equal(Season.Spring, seasons.ActualSeason);
        Assert.Empty(transitions);
    }

    [Fact]
    public void Release_SnapsBackToTheCalendar()
    {
        var (clock, seasons) = Build();
        seasons.Pin(Season.Winter);
        clock.Advance((int)DaysToMinutes(28));

        var transitions = Watch(seasons);
        seasons.Release();

        Assert.Equal(SeasonMode.FollowCalendar, seasons.Mode);
        Assert.Equal(Season.Summer, seasons.ActualSeason);
        Assert.Equal([(Season.Winter, Season.Summer)], transitions);
    }

    [Fact]
    public void Release_WhenAlreadyMatchingTheCalendar_AnnouncesNothing()
    {
        var (_, seasons) = Build();
        seasons.Pin(Season.Spring);

        var transitions = Watch(seasons);
        seasons.Release();

        Assert.Equal(SeasonMode.FollowCalendar, seasons.Mode);
        Assert.Empty(transitions);
    }

    [Fact]
    public void AfterRelease_TheWorldResumesTrackingTheCalendar()
    {
        var (clock, seasons) = Build();
        seasons.Pin(Season.Winter);
        seasons.Release();

        clock.Advance((int)DaysToMinutes(28));

        Assert.Equal(Season.Summer, seasons.ActualSeason);
    }

    [Fact]
    public void ForceSeason_SetsTheWorldWithoutChangingMode()
    {
        var (_, seasons) = Build();
        var transitions = Watch(seasons);

        seasons.ForceSeason(Season.Autumn);

        Assert.Equal(Season.Autumn, seasons.ActualSeason);
        Assert.Equal(SeasonMode.FollowCalendar, seasons.Mode);
        Assert.Equal([(Season.Spring, Season.Autumn)], transitions);
    }

    [Fact]
    public void ForceSeason_InFollowCalendarMode_IsOverwrittenByTheNextRollover()
    {
        // Documents the trade-off: ForceSeason is for scripted moments, not for holding a
        // season. Holding is what Pin is for.
        var (clock, seasons) = Build();
        seasons.ForceSeason(Season.Winter);

        clock.Advance((int)DaysToMinutes(28));

        Assert.Equal(Season.Summer, seasons.ActualSeason);
    }

    [Fact]
    public void ForceSeason_WhilePinned_KeepsThePin()
    {
        var (clock, seasons) = Build();
        seasons.Pin(Season.Winter);
        seasons.ForceSeason(Season.Autumn);

        clock.Advance((int)DaysToMinutes(28));

        Assert.Equal(SeasonMode.Pinned, seasons.Mode);
        Assert.Equal(Season.Autumn, seasons.ActualSeason);
    }

    [Fact]
    public void RestoreState_SetsBothFieldsAndFiresNothing()
    {
        // Load must not look like a season transition — subscribers would fire VFX,
        // swap tilesets, and announce a change that never happened.
        var (_, seasons) = Build();
        var transitions = Watch(seasons);

        seasons.RestoreState(SeasonMode.Pinned, Season.Winter);

        Assert.Equal(SeasonMode.Pinned, seasons.Mode);
        Assert.Equal(Season.Winter, seasons.ActualSeason);
        Assert.Empty(transitions);
    }

    [Fact]
    public void RestoreState_RestoresPinnedBehaviour()
    {
        var (clock, seasons) = Build(DaysToMinutes(GameDate.DaysPerYear));
        seasons.RestoreState(SeasonMode.Pinned, Season.Winter);

        clock.Advance((int)DaysToMinutes(GameDate.DaysPerYear));

        Assert.Equal(Season.Winter, seasons.ActualSeason);
    }

    [Fact]
    public void SeasonChanged_CarriesOldThenNew()
    {
        var (_, seasons) = Build();
        Season? from = null;
        Season? to = null;
        seasons.SeasonChanged += (f, t) => (from, to) = (f, t);

        seasons.Pin(Season.Autumn);

        Assert.Equal(Season.Spring, from);
        Assert.Equal(Season.Autumn, to);
    }
}
