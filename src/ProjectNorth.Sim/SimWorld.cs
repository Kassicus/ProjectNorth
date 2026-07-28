using ProjectNorth.Sim.Calendar;
using ProjectNorth.Sim.Core;
using ProjectNorth.Sim.Save;
using ProjectNorth.Sim.Weather;

namespace ProjectNorth.Sim;

/// <summary>
/// The simulation, as one object. Owns the seed, the master RNG, the clock, the season
/// state machine, and the weather provider.
/// </summary>
/// <remarks>
/// <para>
/// <strong>This is the Bridge's entire view of the simulation</strong> (CLAUDE.md rule 6).
/// Presentation constructs a <see cref="SimWorld"/>, reads state from it, and issues commands
/// to it — it never news up a <see cref="GameClock"/> or a <see cref="SeasonController"/> of
/// its own. Keeping the wiring in one place is what makes it possible to add a system later
/// without every scene in <c>game/</c> learning about it.
/// </para>
/// <para>
/// Nothing here touches Godot, the filesystem, or the wall clock. A <see cref="SimWorld"/>
/// runs identically inside the editor, inside a unit test, and inside the headless balance
/// harness that plays ten in-game years overnight (TECH §6).
/// </para>
/// </remarks>
public sealed class SimWorld
{
    private SimWorld(ulong seed, SimRng rng, GameClock clock)
    {
        Seed = seed;
        Rng = rng;
        Clock = clock;
        Seasons = new SeasonController(clock);

        // M0 placeholder. M1 swaps in the ClimateDirector (TECH §4.2); nothing that reads
        // through IWeatherProvider should notice.
        Weather = new ConstantWeatherProvider();
    }

    /// <summary>
    /// Starts a new run.
    /// </summary>
    /// <param name="seed">
    /// The run seed. Fully determines the run — same seed, same weather, same everything
    /// (CLAUDE.md rule 2).
    /// </param>
    public SimWorld(ulong seed)
        : this(seed, new SimRng(seed), new GameClock())
    {
    }

    /// <summary>The seed this run was started from.</summary>
    public ulong Seed { get; }

    /// <summary>
    /// The master random stream. Systems should take a <see cref="SimRng.Fork"/> of this
    /// rather than drawing from it directly, so their streams stay independent.
    /// </summary>
    public SimRng Rng { get; }

    /// <summary>The authoritative clock. Presentation drives it; everything else listens.</summary>
    public GameClock Clock { get; }

    /// <summary>
    /// The season actually outside. Read this for gameplay — never
    /// <see cref="GameDate.CalendarSeason"/> (CLAUDE.md rule 3).
    /// </summary>
    public SeasonController Seasons { get; }

    /// <summary>The weather model. An M0 constant; the real thing arrives in M1.</summary>
    public IWeatherProvider Weather { get; }

    /// <summary>
    /// Restores a run from a snapshot.
    /// </summary>
    /// <param name="state">A snapshot, already migrated to the current version.</param>
    /// <returns>A world that continues in lockstep with the one that was captured.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="state"/> is null.</exception>
    /// <remarks>
    /// Restoring the season through <c>RestoreState</c> rather than <c>Pin</c> matters: load
    /// must not raise <c>SeasonChanged</c>, or every subscriber runs a transition for a
    /// change that never happened.
    /// </remarks>
    public static SimWorld Restore(SimState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var world = new SimWorld(
            state.Seed,
            SimRng.FromState(state.RngState),
            new GameClock(state.TotalMinutes));

        world.Seasons.RestoreState(state.SeasonMode, state.ActualSeason);

        return world;
    }

    /// <summary>
    /// Captures everything needed to resume this run exactly.
    /// </summary>
    /// <returns>A snapshot stamped with <see cref="SaveSystem.CurrentVersion"/>.</returns>
    public SimState CaptureState() => new(
        Version: SaveSystem.CurrentVersion,
        Seed: Seed,
        RngState: Rng.State,
        TotalMinutes: Clock.TotalMinutes,
        SeasonMode: Seasons.Mode,
        ActualSeason: Seasons.ActualSeason);
}
