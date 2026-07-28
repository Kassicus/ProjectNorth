namespace ProjectNorth.Sim.Save;

/// <summary>
/// Thrown when a save's version cannot be handled by this build.
/// </summary>
/// <remarks>
/// Raised when a save is <em>newer</em> than this build supports (the player rolled back a
/// version, or opened a save from a beta branch), and when a version is missing or nonsense.
/// A save that is merely older is not an error — it goes through the migration chain in
/// <see cref="SaveSystem"/>.
/// <para>
/// The alternative — best-effort loading — is worse than failing. Deserializing a v3 save as
/// v1 produces a world that looks fine and is quietly wrong, and the player finds out ten
/// hours later (CLAUDE.md rule 4).
/// </para>
/// </remarks>
public sealed class SaveVersionException : Exception
{
    /// <summary>
    /// Creates the exception.
    /// </summary>
    /// <param name="saveVersion">The version found in the save, or 0 if absent/unreadable.</param>
    /// <param name="supportedVersion">The newest version this build can read.</param>
    public SaveVersionException(int saveVersion, int supportedVersion)
        : base(BuildMessage(saveVersion, supportedVersion))
    {
        SaveVersion = saveVersion;
        SupportedVersion = supportedVersion;
    }

    /// <summary>The version found in the save file.</summary>
    public int SaveVersion { get; }

    /// <summary>The newest save version this build understands.</summary>
    public int SupportedVersion { get; }

    private static string BuildMessage(int saveVersion, int supportedVersion) =>
        saveVersion > supportedVersion
            ? $"This save was written by a newer version of the game (save version {saveVersion}, " +
              $"this build supports up to {supportedVersion}). Refusing to load it rather than " +
              $"guess at what changed."
            : $"Unsupported save version {saveVersion} (this build supports 1 to {supportedVersion}). " +
              $"The file is missing a version stamp or is not a Project North save.";
}
