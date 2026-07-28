namespace ProjectNorth.Sim.Calendar;

/// <summary>
/// A date on the wall calendar: year, season slot, and day within that season.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Read this before using <see cref="CalendarSeason"/>.</strong> A
/// <see cref="GameDate"/> is what the paper calendar in the cabin <em>says</em>. It is not
/// what is outside the window. The season actually in effect lives on
/// <c>SeasonController.ActualSeason</c>, and the two diverge by script: the False Thaw and
/// the Winter That Stays are precisely the beats where the calendar keeps flipping pages
/// and the world refuses to follow (GDD §3, TECH §3).
/// </para>
/// <para>
/// So: <strong>never derive gameplay season from a date</strong> (CLAUDE.md rule 3). If a
/// system needs to know whether seed will germinate, whether the lake is ice, or how fast
/// warmth drains, it asks the <c>SeasonController</c>. A date is for the calendar UI, for
/// scheduling, and for the plane's cadence — nothing else.
/// </para>
/// <para>
/// Calendar shape is GDD §3 (decided v0.3): 28-day seasons, 4 seasons, so a year is 112
/// days / 16 weeks / 16 plane visits.
/// </para>
/// </remarks>
/// <param name="Year">The year, starting at 1.</param>
/// <param name="CalendarSeason">
/// The season slot the calendar is showing — <em>not</em> necessarily the weather outside.
/// </param>
/// <param name="DayOfSeason">The day within the season, 1 to <see cref="DaysPerSeason"/>.</param>
public readonly record struct GameDate(int Year, Season CalendarSeason, int DayOfSeason)
{
    /// <summary>Days in every season (GDD §3).</summary>
    public const int DaysPerSeason = 28;

    /// <summary>Seasons in a year.</summary>
    public const int SeasonsPerYear = 4;

    /// <summary>Days in a year — <see cref="DaysPerSeason"/> × <see cref="SeasonsPerYear"/>.</summary>
    public const int DaysPerYear = DaysPerSeason * SeasonsPerYear;

    /// <summary>Days in a week. The plane's cadence (GDD §4).</summary>
    public const int DaysPerWeek = 7;

    /// <summary>The year, starting at 1.</summary>
    public int Year { get; } = Year >= 1
        ? Year
        : throw new ArgumentOutOfRangeException(nameof(Year), Year, "Year starts at 1.");

    /// <summary>The day within the season, 1 to <see cref="DaysPerSeason"/>.</summary>
    public int DayOfSeason { get; } = DayOfSeason is >= 1 and <= DaysPerSeason
        ? DayOfSeason
        : throw new ArgumentOutOfRangeException(
            nameof(DayOfSeason), DayOfSeason, $"DayOfSeason runs 1..{DaysPerSeason}.");

    /// <summary>
    /// Days elapsed since the epoch — day 0 being Year 1, Spring, Day 1 (arrival).
    /// Inverse of <see cref="FromTotalDays"/>.
    /// </summary>
    public int TotalDays =>
        ((Year - 1) * DaysPerYear) + ((int)CalendarSeason * DaysPerSeason) + (DayOfSeason - 1);

    /// <summary>The week within the season, 1 to 4.</summary>
    public int WeekOfSeason => ((DayOfSeason - 1) / DaysPerWeek) + 1;

    /// <summary>
    /// True on days 7, 14, 21, and 28 — the plane days. Sixteen a year (GDD §3).
    /// </summary>
    public bool IsWeekBoundary => DayOfSeason % DaysPerWeek == 0;

    /// <summary>
    /// Builds a date from days elapsed since the epoch, where day 0 is Year 1, Spring, Day 1.
    /// </summary>
    /// <param name="totalDays">Days since the epoch. Must not be negative.</param>
    /// <returns>The corresponding calendar date.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="totalDays"/> is negative. Day 0 is arrival; the
    /// simulation has no representation for time before the game began.
    /// </exception>
    public static GameDate FromTotalDays(int totalDays)
    {
        if (totalDays < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(totalDays), totalDays, "Day 0 is the epoch (Y1 Spring 1); there are no negative days.");
        }

        var year = (totalDays / DaysPerYear) + 1;
        var dayOfYear = totalDays % DaysPerYear;

        return new GameDate(
            year,
            (Season)(dayOfYear / DaysPerSeason),
            (dayOfYear % DaysPerSeason) + 1);
    }

    /// <summary>Renders the date as it reads on the calendar, e.g. <c>"Y1 Spring 12"</c>.</summary>
    /// <returns>A human-readable date string.</returns>
    public override string ToString() => $"Y{Year} {CalendarSeason} {DayOfSeason}";
}
