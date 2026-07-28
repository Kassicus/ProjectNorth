namespace ProjectNorth.Sim.Weather;

/// <summary>
/// Local weather conditions at one point in space and time.
/// </summary>
/// <remarks>
/// The resolved output of the weather model: base seasonal climate blended with every
/// overlapping weather system (TECH §4.2). Presentation drives particles, shaders,
/// CanvasModulate, and the wind audio bed from this; the temperature model, the exposure
/// ladder, and the barometer all read it too.
/// </remarks>
/// <param name="TemperatureC">Ambient air temperature in degrees Celsius, before wind chill.</param>
/// <param name="WindSpeedKph">Wind speed in km/h.</param>
/// <param name="WindDirectionDeg">
/// Compass bearing the wind blows <em>towards</em>, in degrees clockwise from north.
/// Prevailing weather runs NW→SE (TECH §4.2), so a typical front reads near 135.
/// </param>
/// <param name="PrecipitationIntensity">
/// Precipitation rate, 0 (dry) to 1 (whiteout/downpour). Whether it falls as rain or snow
/// is a function of <paramref name="TemperatureC"/>, not a separate field.
/// </param>
/// <param name="Visibility01">
/// How far the player can see, 1 (clear) down to 0 (whiteout). Drives the hard visibility
/// radius that makes getting lost a mechanic (TECH §4.4).
/// </param>
public readonly record struct WeatherSample(
    float TemperatureC,
    float WindSpeedKph,
    float WindDirectionDeg,
    float PrecipitationIntensity,
    float Visibility01)
{
    /// <summary>
    /// A calm, clear spring day — the M0 placeholder and a sane default for tests.
    /// </summary>
    public static WeatherSample MildSpringDay => new(
        TemperatureC: 8f,
        WindSpeedKph: 6f,
        WindDirectionDeg: 135f,
        PrecipitationIntensity: 0f,
        Visibility01: 1f);
}
