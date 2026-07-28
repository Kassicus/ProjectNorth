namespace ProjectNorth.Sim.Calendar;

/// <summary>
/// The authoritative tick source for the simulation (TECH §3).
/// </summary>
/// <remarks>
/// <para>
/// Time is counted in <em>sim minutes</em>. The clock has no opinion about real time —
/// Presentation decides how many sim minutes a real second is worth and feeds the result
/// to <see cref="Advance"/>. Nothing here ever reads the wall clock (CLAUDE.md rule 2);
/// a run's history is a pure function of its starting state and the minutes handed to it.
/// </para>
/// <para>
/// <strong>Advance is not a jump.</strong> A single call spanning weeks walks forward
/// day by day internally, firing every boundary it crosses, once each, in chronological
/// order. The event layer (anomalies, plane visits, story beats) subscribes here, so a
/// season advanced in one call still owes the world its four plane days.
/// </para>
/// </remarks>
public sealed class GameClock
{
    /// <summary>Sim minutes in a day.</summary>
    public const int MinutesPerDay = 24 * 60;

    private long _totalMinutes;

    /// <summary>
    /// Creates a clock, optionally resuming mid-run from a saved minute count.
    /// </summary>
    /// <param name="totalMinutes">Sim minutes since the epoch (Y1 Spring 1, 00:00).</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="totalMinutes"/> is negative.
    /// </exception>
    public GameClock(long totalMinutes = 0)
    {
        if (totalMinutes < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(totalMinutes), totalMinutes, "The clock starts at the epoch; time before it does not exist.");
        }

        _totalMinutes = totalMinutes;
    }

    /// <summary>
    /// Raised once for each day that finishes, carrying <em>the date of the day that ended</em>.
    /// </summary>
    public event Action<GameDate>? DayEnded;

    /// <summary>
    /// Raised when a completed day was a week boundary — day 7, 14, 21, or 28 — carrying
    /// <em>the date of the day that ended</em>. This is the plane's cadence (GDD §4).
    /// </summary>
    public event Action<GameDate>? WeekEnded;

    /// <summary>
    /// Raised when the calendar turns to a new season, carrying <em>the new date</em>.
    /// </summary>
    /// <remarks>
    /// This says the calendar page turned. It does <strong>not</strong> say the weather
    /// changed — <c>SeasonController</c> decides whether the world follows the calendar
    /// (CLAUDE.md rule 3).
    /// </remarks>
    public event Action<GameDate>? CalendarSeasonChanged;

    /// <summary>
    /// Raised when the calendar turns to a new year, carrying <em>the new date</em>.
    /// </summary>
    public event Action<GameDate>? YearEnded;

    /// <summary>Sim minutes elapsed since the epoch.</summary>
    public long TotalMinutes => _totalMinutes;

    /// <summary>Minutes since midnight, 0 to <see cref="MinutesPerDay"/> − 1.</summary>
    public int MinuteOfDay => (int)(_totalMinutes % MinutesPerDay);

    /// <summary>Whole days elapsed since the epoch.</summary>
    public int TotalDays => (int)(_totalMinutes / MinutesPerDay);

    /// <summary>The current wall-calendar date. See <see cref="GameDate"/> before using its season.</summary>
    public GameDate CurrentDate => GameDate.FromTotalDays(TotalDays);

    /// <summary>
    /// Moves time forward, firing every boundary crossed along the way.
    /// </summary>
    /// <param name="minutes">Sim minutes to advance. Zero is a no-op.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="minutes"/> is negative. Time does not run backwards —
    /// a rewind would have to un-fire events that systems have already acted on.
    /// </exception>
    public void Advance(int minutes)
    {
        if (minutes < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minutes), minutes, "Time only moves forward.");
        }

        var remaining = minutes;
        while (remaining > 0)
        {
            var minutesLeftInDay = MinutesPerDay - MinuteOfDay;

            if (remaining < minutesLeftInDay)
            {
                _totalMinutes += remaining;
                return;
            }

            // Land exactly on midnight so subscribers observing the clock during the event
            // see a consistent date, then announce the day that just finished.
            _totalMinutes += minutesLeftInDay;
            remaining -= minutesLeftInDay;
            RaiseDayBoundary();
        }
    }

    /// <summary>
    /// Fires one day's worth of boundary events, in the fixed order:
    /// day, then week, then season, then year — narrow to broad, so a subscriber handling
    /// "the year turned" can rely on the day and season handlers having already run.
    /// </summary>
    private void RaiseDayBoundary()
    {
        var newDate = CurrentDate;
        var endedDate = GameDate.FromTotalDays(newDate.TotalDays - 1);

        DayEnded?.Invoke(endedDate);

        if (endedDate.IsWeekBoundary)
        {
            WeekEnded?.Invoke(endedDate);
        }

        if (newDate.CalendarSeason != endedDate.CalendarSeason)
        {
            CalendarSeasonChanged?.Invoke(newDate);
        }

        if (newDate.Year != endedDate.Year)
        {
            YearEnded?.Invoke(newDate);
        }
    }
}
