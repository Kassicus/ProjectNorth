using ProjectNorth.Sim.Calendar;
using ProjectNorth.Sim.Save;

namespace ProjectNorth.Sim.Tests.Save;

/// <summary>
/// CLAUDE.md rule 4: saves are versioned from the first write, and a save is never silently
/// reinterpreted. Retrofitting migration onto a shipped save format is misery, so the
/// machinery exists before there is anything worth saving.
/// </summary>
public class SaveSystemTests
{
    private static SimState SampleState() => new(
        Version: SaveSystem.CurrentVersion,
        Seed: 4471,
        RngState: 0xDEADBEEFCAFEF00D,
        TotalMinutes: 123_456,
        SeasonMode: SeasonMode.Pinned,
        ActualSeason: Season.Winter);

    [Fact]
    public void CurrentVersion_StartsAtOne()
    {
        Assert.Equal(1, SaveSystem.CurrentVersion);
    }

    [Fact]
    public void RoundTrip_PreservesEveryField()
    {
        var original = SampleState();

        var restored = SaveSystem.Deserialize(SaveSystem.Serialize(original));

        Assert.Equal(original, restored);
    }

    [Fact]
    public void Serialize_WritesEnumsAsNamesNotNumbers()
    {
        // Saves are a debugging surface. "Winter" survives a refactor that renumbers an
        // enum; "3" quietly becomes a different season.
        var json = SaveSystem.Serialize(SampleState());

        Assert.Contains("\"Winter\"", json);
        Assert.Contains("\"Pinned\"", json);
    }

    [Fact]
    public void Serialize_WritesIndentedJson()
    {
        Assert.Contains("\n", SaveSystem.Serialize(SampleState()));
    }

    [Fact]
    public void Serialize_AlwaysStampsTheVersion()
    {
        Assert.Contains("\"Version\"", SaveSystem.Serialize(SampleState()));
    }

    [Fact]
    public void Deserialize_RejectsANewerSaveLoudly()
    {
        var json = SaveSystem.Serialize(SampleState())
            .Replace("\"Version\": 1", "\"Version\": 999", StringComparison.Ordinal);

        var ex = Assert.Throws<SaveVersionException>(() => SaveSystem.Deserialize(json));

        Assert.Equal(999, ex.SaveVersion);
        Assert.Equal(SaveSystem.CurrentVersion, ex.SupportedVersion);
    }

    [Fact]
    public void Deserialize_RejectsANonsenseVersion()
    {
        var json = SaveSystem.Serialize(SampleState())
            .Replace("\"Version\": 1", "\"Version\": 0", StringComparison.Ordinal);

        Assert.Throws<SaveVersionException>(() => SaveSystem.Deserialize(json));
    }

    [Fact]
    public void Deserialize_RejectsASaveWithNoVersionAtAll()
    {
        Assert.Throws<SaveVersionException>(() => SaveSystem.Deserialize("""{ "Seed": 1 }"""));
    }

    [Fact]
    public void Deserialize_RejectsMalformedJson()
    {
        Assert.Throws<SaveCorruptException>(() => SaveSystem.Deserialize("not json at all"));
    }

    [Fact]
    public void Deserialize_RejectsNullAndEmptyInput()
    {
        Assert.Throws<SaveCorruptException>(() => SaveSystem.Deserialize(""));
        Assert.Throws<SaveCorruptException>(() => SaveSystem.Deserialize("   "));
        Assert.Throws<SaveCorruptException>(() => SaveSystem.Deserialize("null"));
    }

    [Fact]
    public void SaveVersionException_ExplainsItselfInTheMessage()
    {
        var ex = new SaveVersionException(7, 1);

        Assert.Contains("7", ex.Message, StringComparison.Ordinal);
        Assert.Contains("1", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void CapturedStateStampsTheCurrentVersion()
    {
        // Nothing should ever write a save without a version on it.
        var json = SaveSystem.Serialize(SampleState() with { Version = SaveSystem.CurrentVersion });

        Assert.Equal(SaveSystem.CurrentVersion, SaveSystem.Deserialize(json).Version);
    }
}
