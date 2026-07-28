using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProjectNorth.Sim.Save;

/// <summary>
/// Reads and writes <see cref="SimState"/>, with versioning and migration.
/// </summary>
/// <remarks>
/// <para>
/// CLAUDE.md rule 4. The whole point of this class existing at M0, when the save contains
/// almost nothing, is that the discipline has to predate the content: the version stamp, the
/// loud rejection of newer saves, and the migration chain are all cheap now and impossible
/// to retrofit once players have save files.
/// </para>
/// <para>
/// This class deals in strings, not files. Where a save lives on disk is a Presentation
/// concern (Godot's <c>user://</c>); Sim must stay headless (CLAUDE.md rule 1).
/// </para>
/// </remarks>
public static class SaveSystem
{
    /// <summary>
    /// The save format version this build writes and can read up to.
    /// </summary>
    /// <remarks>
    /// Bump this in the same commit as any change to <see cref="SimState"/>'s shape, and add
    /// the matching migration step below. Never change <see cref="SimState"/> without it.
    /// </remarks>
    public const int CurrentVersion = 1;

    /// <summary>The oldest version the migration chain can start from.</summary>
    private const int OldestSupportedVersion = 1;

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>
    /// Writes a snapshot to JSON.
    /// </summary>
    /// <param name="state">The snapshot to write.</param>
    /// <returns>Indented JSON with enums written as names.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="state"/> is null.</exception>
    public static string Serialize(SimState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        return JsonSerializer.Serialize(state, Options);
    }

    /// <summary>
    /// Reads a snapshot from JSON, checking the version before anything else and migrating
    /// older formats forward.
    /// </summary>
    /// <param name="json">The save contents.</param>
    /// <returns>A snapshot at <see cref="CurrentVersion"/>.</returns>
    /// <exception cref="SaveVersionException">
    /// Thrown when the save is newer than this build supports, or its version is missing or
    /// out of range. Never reinterpreted, never best-effort.
    /// </exception>
    /// <exception cref="SaveCorruptException">Thrown when the save is not parseable JSON.</exception>
    public static SimState Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new SaveCorruptException("The save is empty.");
        }

        // Version first, always — before any attempt to interpret the rest of the document.
        var version = ReadVersion(json);

        if (version > CurrentVersion || version < OldestSupportedVersion)
        {
            throw new SaveVersionException(version, CurrentVersion);
        }

        SimState? state;
        try
        {
            state = JsonSerializer.Deserialize<SimState>(json, Options);
        }
        catch (JsonException ex)
        {
            throw new SaveCorruptException("The save could not be parsed.", ex);
        }

        if (state is null)
        {
            throw new SaveCorruptException("The save contained no state.");
        }

        return Migrate(state);
    }

    /// <summary>
    /// Pulls the version stamp out of the document without deserializing the rest, so a save
    /// whose body this build cannot understand still fails with a useful message.
    /// </summary>
    private static int ReadVersion(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);

            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new SaveCorruptException("The save is not a JSON object.");
            }

            if (!document.RootElement.TryGetProperty("Version", out var versionElement) &&
                !document.RootElement.TryGetProperty("version", out versionElement))
            {
                // No stamp at all: either predates versioning (impossible — versioning came
                // first) or is not one of our files. Either way, do not guess.
                throw new SaveVersionException(0, CurrentVersion);
            }

            if (!versionElement.TryGetInt32(out var version))
            {
                throw new SaveVersionException(0, CurrentVersion);
            }

            return version;
        }
        catch (JsonException ex)
        {
            throw new SaveCorruptException("The save could not be parsed.", ex);
        }
    }

    /// <summary>
    /// Walks a snapshot forward one version at a time until it reaches
    /// <see cref="CurrentVersion"/>.
    /// </summary>
    /// <remarks>
    /// At v1 this loop never runs — that is expected. The structure is here so that adding
    /// v2 is a two-line change (bump <see cref="CurrentVersion"/>, add the case) rather than
    /// a design problem solved under pressure. Each step must be explicit about what it
    /// fills in for fields that did not exist in the older format.
    /// </remarks>
    private static SimState Migrate(SimState state)
    {
        var current = state;

        while (current.Version < CurrentVersion)
        {
            current = current.Version switch
            {
                // 1 => MigrateV1ToV2(current),
                _ => throw new SaveVersionException(current.Version, CurrentVersion),
            };
        }

        return current;
    }
}
