using ProjectNorth.Sim.Core;

namespace ProjectNorth.Sim.Weather;

/// <summary>
/// Answers "what is the weather at this place, at this time?" — the single API through which
/// everything in the game asks about the sky (TECH §4.2).
/// </summary>
/// <remarks>
/// <para>
/// M1 replaces the M0 placeholder with the <c>ClimateDirector</c>: weather systems spawned
/// off-map with position, velocity, radius, and an intensity curve, rolling across the map
/// and blended with the base seasonal climate wherever they overlap. Weather is spatial and
/// moving, never a per-day flag.
/// </para>
/// </remarks>
public interface IWeatherProvider
{
    /// <summary>
    /// Samples the weather at a world position and a sim time.
    /// </summary>
    /// <param name="worldPos">Where to sample.</param>
    /// <param name="timeMinutes">
    /// When to sample, in sim minutes since the epoch. <strong>May be in the future.</strong>
    /// </param>
    /// <returns>The conditions at that place and time.</returns>
    /// <remarks>
    /// <para>
    /// <strong>Future-time queries are legitimate, and this is why.</strong> The simulation
    /// is deterministic and seeded (CLAUDE.md rule 2), so weather at <c>t + 6h</c> is already
    /// decided — asking early does not invent an answer, it reads one. That is what makes
    /// forecasting instruments honest rather than a UI cheat:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>
    ///     <strong>The barometer</strong> is a sanctioned peek at this method a few hours
    ///     ahead, expressed as falling/steady/rising (TECH §4.3).
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Sky reading</strong> renders distant systems on the horizon in their true
    ///     direction of approach — the art is the forecast.
    ///   </description></item>
    /// </list>
    /// <para>
    /// Implementations must therefore be <em>pure</em>: same seed, same position, same time
    /// ⇒ same sample, regardless of call order or how many times it is asked. An
    /// implementation that mutates state on read would make the barometer lie — and in Act 3
    /// the barometer must lie only when the story says so (TECH §4.3, anomalous weather),
    /// never by accident.
    /// </para>
    /// </remarks>
    WeatherSample GetWeather(SimVec2 worldPos, long timeMinutes);
}
