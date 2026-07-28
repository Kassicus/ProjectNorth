namespace ProjectNorth.Sim.Calendar;

/// <summary>
/// Owns the season that is <em>actually outside</em> — as opposed to the one the calendar
/// claims (TECH §3).
/// </summary>
/// <remarks>
/// <para>
/// Season is a scriptable state machine, not a function of the date (CLAUDE.md rule 3).
/// Normally it walks Spring → Summer → Autumn → Winter in step with the calendar, but story
/// phases can hold it in place or move it backwards. Every gameplay system that cares about
/// the season — planting windows, lake ice, warmth drain, wildlife, tile variants — reads
/// <see cref="ActualSeason"/> here, never <see cref="GameDate.CalendarSeason"/>.
/// </para>
/// <para>
/// That split is what lets Act 3 land as mechanical betrayal rather than a cutscene. The two
/// beats it exists for:
/// </para>
/// <list type="bullet">
///   <item><description>
///     <strong>The Winter That Stays</strong> — <c>Pin(Season.Winter)</c>. The calendar runs
///     on into calendar-summer; the world stays frozen.
///   </description></item>
///   <item><description>
///     <strong>The False Thaw</strong> — <c>Pin(Season.Spring)</c>, let ten days of real
///     thaw run, then <c>Pin(Season.Winter)</c>. Spring reverses into winter with the
///     calendar still reading Spring. A season transition that runs <em>backwards</em> is a
///     legal state change here, which is exactly why this is built before any gameplay is.
///   </description></item>
/// </list>
/// </remarks>
public sealed class SeasonController
{
    private readonly GameClock _clock;
    private Season _actualSeason;
    private SeasonMode _mode;

    /// <summary>
    /// Creates a controller that follows <paramref name="clock"/>, adopting its current
    /// calendar season as the starting weather.
    /// </summary>
    /// <param name="clock">The authoritative clock to track.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="clock"/> is null.</exception>
    public SeasonController(GameClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        _clock = clock;
        _mode = SeasonMode.FollowCalendar;
        _actualSeason = clock.CurrentDate.CalendarSeason;

        _clock.CalendarSeasonChanged += OnCalendarSeasonChanged;
    }

    /// <summary>
    /// Raised when the season actually outside changes, carrying (old, new).
    /// </summary>
    /// <remarks>
    /// Fires only on real changes, and never on <see cref="RestoreState"/> — loading a save
    /// must not look like the world transitioning.
    /// </remarks>
    public event Action<Season, Season>? SeasonChanged;

    /// <summary>The season actually in effect in the world. The one gameplay reads.</summary>
    public Season ActualSeason => _actualSeason;

    /// <summary>Whether the world is currently following the calendar or held in place.</summary>
    public SeasonMode Mode => _mode;

    /// <summary>
    /// Holds the world at <paramref name="season"/> and ignores calendar rollovers from here on.
    /// </summary>
    /// <param name="season">The season to hold.</param>
    public void Pin(Season season)
    {
        _mode = SeasonMode.Pinned;
        SetActualSeason(season);
    }

    /// <summary>
    /// Sets the season outside without changing <see cref="Mode"/> — for scripted moments.
    /// </summary>
    /// <param name="season">The season to switch to.</param>
    /// <remarks>
    /// In <see cref="SeasonMode.FollowCalendar"/> this is temporary: the next calendar
    /// rollover overwrites it. Use <see cref="Pin"/> to make a season stick.
    /// </remarks>
    public void ForceSeason(Season season) => SetActualSeason(season);

    /// <summary>
    /// Resumes following the calendar, snapping the world to the current calendar season.
    /// </summary>
    public void Release()
    {
        _mode = SeasonMode.FollowCalendar;
        SetActualSeason(_clock.CurrentDate.CalendarSeason);
    }

    /// <summary>
    /// Restores saved state without raising <see cref="SeasonChanged"/>.
    /// </summary>
    /// <param name="mode">The saved mode.</param>
    /// <param name="season">The saved actual season.</param>
    /// <remarks>
    /// The silence is the point. Subscribers react to <see cref="SeasonChanged"/> by swapping
    /// tilesets and running transition VFX; on load the world should simply already be that way.
    /// </remarks>
    public void RestoreState(SeasonMode mode, Season season)
    {
        _mode = mode;
        _actualSeason = season;
    }

    private void OnCalendarSeasonChanged(GameDate newDate)
    {
        if (_mode == SeasonMode.FollowCalendar)
        {
            SetActualSeason(newDate.CalendarSeason);
        }
    }

    private void SetActualSeason(Season season)
    {
        if (_actualSeason == season)
        {
            return;
        }

        var previous = _actualSeason;
        _actualSeason = season;
        SeasonChanged?.Invoke(previous, season);
    }
}
