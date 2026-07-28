using ProjectNorth.Sim.Core;

namespace ProjectNorth.Sim.Weather;

/// <summary>
/// The M0 placeholder: one fixed sample, everywhere, always.
/// </summary>
/// <remarks>
/// Exists so <c>SimWorld</c> can expose a real <see cref="IWeatherProvider"/> before the
/// weather model is written, and so the Bridge can be built against the final interface.
/// M1 replaces it with the <c>ClimateDirector</c> (TECH §4.2) — nothing that consumes
/// <see cref="IWeatherProvider"/> should need to change when that happens.
/// <para>
/// It also happens to satisfy the purity requirement trivially, which makes it a useful
/// control in tests.
/// </para>
/// </remarks>
public sealed class ConstantWeatherProvider : IWeatherProvider
{
    private readonly WeatherSample _sample;

    /// <summary>
    /// Creates a provider that always reports <paramref name="sample"/>.
    /// </summary>
    /// <param name="sample">The conditions to report. Defaults to a mild spring day.</param>
    public ConstantWeatherProvider(WeatherSample? sample = null)
    {
        _sample = sample ?? WeatherSample.MildSpringDay;
    }

    /// <inheritdoc />
    public WeatherSample GetWeather(SimVec2 worldPos, long timeMinutes) => _sample;
}
