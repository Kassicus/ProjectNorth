using ProjectNorth.Sim.Calendar;

namespace ProjectNorth.Sim.Save;

/// <summary>
/// A complete, serializable snapshot of the simulation. This is the save file.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Every future Sim system adds its state here</strong> — needs, temperature,
/// economy, world flags, the event layer's fired-beat set, the player's map record. When it
/// does, the shape of this record changes, and per CLAUDE.md rule 4 that means
/// <see cref="SaveSystem.CurrentVersion"/> goes up and a migration is written in the same
/// commit. No exceptions: a save written by any build must either load correctly or be
/// rejected out loud.
/// </para>
/// <para>
/// Note what is <em>not</em> here: no derived values. The date is not stored, because
/// <see cref="TotalMinutes"/> determines it. Storing both invites them to disagree.
/// </para>
/// </remarks>
/// <param name="Version">
/// The save format version this snapshot was written by. Read before anything else.
/// </param>
/// <param name="Seed">The run seed. With the RNG state, this reproduces the run exactly.</param>
/// <param name="RngState">
/// The master generator's position in its stream. Restoring this is what lets a loaded game
/// continue in lockstep rather than merely resembling the one that was saved.
/// </param>
/// <param name="TotalMinutes">Sim minutes elapsed since the epoch.</param>
/// <param name="SeasonMode">Whether the world was following the calendar or pinned.</param>
/// <param name="ActualSeason">
/// The season actually outside — which in Act 3 is not the one the calendar shows
/// (CLAUDE.md rule 3), so it must be saved explicitly rather than recomputed from the date.
/// </param>
public sealed record SimState(
    int Version,
    ulong Seed,
    ulong RngState,
    long TotalMinutes,
    SeasonMode SeasonMode,
    Season ActualSeason);
