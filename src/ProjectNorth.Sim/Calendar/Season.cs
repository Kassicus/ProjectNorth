namespace ProjectNorth.Sim.Calendar;

/// <summary>
/// The four seasons, in the order a year runs through them.
/// </summary>
/// <remarks>
/// The declaration order is load-bearing: <see cref="GameDate.FromTotalDays"/> casts a
/// season <em>slot index</em> straight to this enum. Do not reorder or renumber.
/// <para>
/// This enum is used for two different things that must never be conflated (CLAUDE.md
/// rule 3): the season the wall calendar shows (<see cref="GameDate.CalendarSeason"/>) and
/// the season actually outside (<c>SeasonController.ActualSeason</c>). In Act 3 they
/// disagree on purpose.
/// </para>
/// </remarks>
public enum Season
{
    /// <summary>Arrival, break-up, planting windows. Y1 begins here (GDD §3).</summary>
    Spring = 0,

    /// <summary>The long days. Act 1's foothold becomes a homestead.</summary>
    Summer = 1,

    /// <summary>Harvest and preparation; the first anomalies turn suspicious.</summary>
    Autumn = 2,

    /// <summary>Freeze-up, floats to skis — and eventually the season that stays.</summary>
    Winter = 3,
}
