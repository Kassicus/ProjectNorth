using ProjectNorth.Sim;
using ProjectNorth.Sim.Calendar;
using ProjectNorth.Sim.Core;
using ProjectNorth.Sim.Save;
using ProjectNorth.Sim.Weather;

namespace ProjectNorth.Sim.Tests.Save;

public class SimWorldTests
{
    [Fact]
    public void NewWorld_StartsAtTheEpochFollowingTheCalendar()
    {
        var world = new SimWorld(4471);

        Assert.Equal(4471UL, world.Seed);
        Assert.Equal(0, world.Clock.TotalMinutes);
        Assert.Equal(new GameDate(1, Season.Spring, 1), world.Clock.CurrentDate);
        Assert.Equal(SeasonMode.FollowCalendar, world.Seasons.Mode);
        Assert.Equal(Season.Spring, world.Seasons.ActualSeason);
        Assert.NotNull(world.Weather);
    }

    [Fact]
    public void SameSeed_ProducesIdenticalWorlds()
    {
        Assert.Equal(new SimWorld(99).Rng.NextUInt64(), new SimWorld(99).Rng.NextUInt64());
    }

    [Fact]
    public void CaptureState_StampsTheCurrentSaveVersion()
    {
        Assert.Equal(SaveSystem.CurrentVersion, new SimWorld(1).CaptureState().Version);
    }

    [Fact]
    public void CaptureState_RecordsClockSeasonAndRngPosition()
    {
        var world = new SimWorld(4471);
        world.Clock.Advance(90 * GameClock.MinutesPerDay);
        world.Seasons.Pin(Season.Winter);
        world.Rng.NextUInt64();

        var state = world.CaptureState();

        Assert.Equal(4471UL, state.Seed);
        Assert.Equal(world.Rng.State, state.RngState);
        Assert.Equal(90L * GameClock.MinutesPerDay, state.TotalMinutes);
        Assert.Equal(SeasonMode.Pinned, state.SeasonMode);
        Assert.Equal(Season.Winter, state.ActualSeason);
    }

    [Fact]
    public void Restore_RebuildsClockDateModeAndSeason()
    {
        var original = new SimWorld(4471);
        original.Clock.Advance(140 * GameClock.MinutesPerDay);
        original.Seasons.Pin(Season.Winter);

        var restored = SimWorld.Restore(SaveSystem.Deserialize(
            SaveSystem.Serialize(original.CaptureState())));

        Assert.Equal(original.Seed, restored.Seed);
        Assert.Equal(original.Clock.TotalMinutes, restored.Clock.TotalMinutes);
        Assert.Equal(original.Clock.CurrentDate, restored.Clock.CurrentDate);
        Assert.Equal(original.Seasons.Mode, restored.Seasons.Mode);
        Assert.Equal(original.Seasons.ActualSeason, restored.Seasons.ActualSeason);
    }

    [Fact]
    public void Restore_DoesNotAnnounceASeasonChange()
    {
        var original = new SimWorld(1);
        original.Seasons.Pin(Season.Winter);
        var state = original.CaptureState();

        var restored = SimWorld.Restore(state);
        var transitions = new List<(Season, Season)>();
        restored.Seasons.SeasonChanged += (f, t) => transitions.Add((f, t));

        Assert.Equal(Season.Winter, restored.Seasons.ActualSeason);
        Assert.Empty(transitions);
    }

    /// <summary>
    /// The M0 Definition of Done: a restored world must continue in lockstep with the
    /// original. Not "close enough" — bit-identical, because determinism is what makes
    /// weather forecastable and the balance harness meaningful (CLAUDE.md rule 2).
    /// </summary>
    [Fact]
    public void RestoredWorld_ContinuesInLockstepWithTheOriginal()
    {
        var original = new SimWorld(20260728);
        original.Clock.Advance(37 * GameClock.MinutesPerDay);
        original.Rng.NextUInt64();
        original.Rng.NextUInt64();

        var restored = SimWorld.Restore(SaveSystem.Deserialize(
            SaveSystem.Serialize(original.CaptureState())));

        // Identical subsequent RNG draws.
        for (var i = 0; i < 500; i++)
        {
            Assert.Equal(original.Rng.NextUInt64(), restored.Rng.NextUInt64());
        }

        // Identical dates after identical Advances.
        for (var i = 0; i < 200; i++)
        {
            original.Clock.Advance(517);
            restored.Clock.Advance(517);
            Assert.Equal(original.Clock.CurrentDate, restored.Clock.CurrentDate);
            Assert.Equal(original.Clock.MinuteOfDay, restored.Clock.MinuteOfDay);
        }
    }

    /// <summary>
    /// Forked streams have to survive a save too — otherwise the weather after loading is a
    /// different weather, and a forecast made before saving becomes a lie.
    /// </summary>
    [Fact]
    public void RestoredWorld_ProducesIdenticalForkedStreams()
    {
        var original = new SimWorld(555);
        original.Rng.NextUInt64();

        var restored = SimWorld.Restore(original.CaptureState());

        var originalWeather = original.Rng.Fork(1);
        var restoredWeather = restored.Rng.Fork(1);

        for (var i = 0; i < 100; i++)
        {
            Assert.Equal(originalWeather.NextUInt64(), restoredWeather.NextUInt64());
        }
    }

    [Fact]
    public void RestoredWorld_KeepsThePinnedSeasonAcrossFurtherCalendarRollovers()
    {
        // Load into the Winter That Stays and keep staying.
        var original = new SimWorld(1);
        original.Clock.Advance(84 * GameClock.MinutesPerDay);
        original.Seasons.Pin(Season.Winter);

        var restored = SimWorld.Restore(original.CaptureState());
        restored.Clock.Advance(2 * GameDate.DaysPerSeason * GameClock.MinutesPerDay);

        Assert.Equal(Season.Summer, restored.Clock.CurrentDate.CalendarSeason);
        Assert.Equal(Season.Winter, restored.Seasons.ActualSeason);
    }

    [Fact]
    public void Restore_RejectsANullState()
    {
        Assert.Throws<ArgumentNullException>(() => SimWorld.Restore(null!));
    }

    [Fact]
    public void Weather_AnswersThroughTheProviderInterface()
    {
        var world = new SimWorld(1);

        var sample = world.Weather.GetWeather(SimVec2.Zero, world.Clock.TotalMinutes);

        Assert.Equal(WeatherSample.MildSpringDay, sample);
    }
}
