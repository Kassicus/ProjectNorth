namespace ProjectNorth.Sim.Save;

/// <summary>
/// Thrown when a save cannot be parsed at all — truncated, empty, or not JSON.
/// </summary>
/// <remarks>
/// Distinct from <see cref="SaveVersionException"/>, which means "parsed fine, wrong
/// version". Presentation wants to tell those apart: a version mismatch is a recoverable
/// "update the game" message, corruption is not.
/// </remarks>
public sealed class SaveCorruptException : Exception
{
    /// <summary>
    /// Creates the exception.
    /// </summary>
    /// <param name="message">What went wrong.</param>
    /// <param name="innerException">The underlying parse failure, if any.</param>
    public SaveCorruptException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
