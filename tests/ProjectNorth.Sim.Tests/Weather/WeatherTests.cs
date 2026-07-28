using ProjectNorth.Sim.Core;
using ProjectNorth.Sim.Weather;

namespace ProjectNorth.Sim.Tests.Weather;

/// <summary>
/// M0 holds the weather API shape only — the real front-based model is M1 (TECH §4.2).
/// These tests pin the contract the ClimateDirector will have to honour when it replaces
/// <see cref="ConstantWeatherProvider"/>.
/// </summary>
public class WeatherTests
{
    [Fact]
    public void MildSpringDay_IsAPlausibleCalmDay()
    {
        var sample = WeatherSample.MildSpringDay;

        Assert.InRange(sample.TemperatureC, -5f, 25f);
        Assert.InRange(sample.WindSpeedKph, 0f, 30f);
        Assert.InRange(sample.WindDirectionDeg, 0f, 360f);
        Assert.Equal(0f, sample.PrecipitationIntensity);
        Assert.Equal(1f, sample.Visibility01);
    }

    [Fact]
    public void ConstantProvider_IgnoresPosition()
    {
        var provider = new ConstantWeatherProvider(WeatherSample.MildSpringDay);

        Assert.Equal(
            provider.GetWeather(SimVec2.Zero, 0),
            provider.GetWeather(new SimVec2(9000f, -4200f), 0));
    }

    [Fact]
    public void ConstantProvider_IgnoresTime()
    {
        var provider = new ConstantWeatherProvider(WeatherSample.MildSpringDay);

        Assert.Equal(
            provider.GetWeather(SimVec2.Zero, 0),
            provider.GetWeather(SimVec2.Zero, 500_000));
    }

    /// <summary>
    /// The barometer contract in miniature (TECH §4.3): a provider must answer for a time
    /// that has not happened yet. Determinism is what makes that a legitimate question
    /// rather than a lie — the answer given now is the answer the world will give then.
    /// </summary>
    [Fact]
    public void Provider_AnswersFutureTimeQueries()
    {
        IWeatherProvider provider = new ConstantWeatherProvider(WeatherSample.MildSpringDay);
        const long now = 1_000L;

        var forecast = provider.GetWeather(SimVec2.Zero, now + (6 * 60));
        var actual = provider.GetWeather(SimVec2.Zero, now + (6 * 60));

        Assert.Equal(forecast, actual);
    }

    [Fact]
    public void ConstantProvider_DefaultsToTheMildSpringDay()
    {
        Assert.Equal(
            WeatherSample.MildSpringDay,
            new ConstantWeatherProvider().GetWeather(SimVec2.Zero, 0));
    }

    [Fact]
    public void Sample_HasValueEquality()
    {
        var a = new WeatherSample(-12f, 40f, 315f, 0.8f, 0.2f);
        var b = new WeatherSample(-12f, 40f, 315f, 0.8f, 0.2f);

        Assert.Equal(a, b);
        Assert.NotEqual(a, a with { Visibility01 = 1f });
    }
}
