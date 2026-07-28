namespace ProjectNorth.Sim.Calendar;

/// <summary>
/// How <see cref="SeasonController"/> decides what season is actually outside.
/// </summary>
public enum SeasonMode
{
    /// <summary>
    /// The world follows the wall calendar. Normal service — Acts 1 and 2.
    /// </summary>
    FollowCalendar = 0,

    /// <summary>
    /// The world is held at a fixed season and calendar rollovers are ignored. The calendar
    /// keeps flipping pages; nothing outside changes. This is Act 3's Winter That Stays.
    /// </summary>
    Pinned = 1,
}
