namespace ProjectNorth.Sim.Core;

/// <summary>
/// The simulation's only source of randomness: a deterministic, seeded, save-restorable
/// pseudo-random number generator.
/// </summary>
/// <remarks>
/// <para>
/// CLAUDE.md rule 2 — determinism is sacred. Sim code must never reach for
/// <c>System.Random</c>, <c>Guid.NewGuid()</c>, or wall-clock time; every random value in
/// the game descends from a <see cref="SimRng"/> whose ancestry traces back to the run
/// seed. Two consequences the design leans on hard:
/// </para>
/// <list type="bullet">
///   <item><description>
///     Weather is <em>forecastable</em>. Because the stream is fixed, the weather model can
///     be evaluated at a future time and give the same answer it will give when that time
///     arrives — which is exactly what the barometer reads (TECH §4.2/§4.3).
///   </description></item>
///   <item><description>
///     The balance harness is reproducible. Running "unprepared forager, seed 4471" twice
///     produces the same ten in-game years (TECH §6).
///   </description></item>
/// </list>
/// <para>
/// The generator is xorshift64*, seeded through splitmix64. It is not cryptographic and
/// does not need to be; it needs to be fast, dependency-free, and byte-stable forever.
/// </para>
/// <para>
/// <strong>Use <see cref="Fork"/>, one stream per system.</strong> If weather, wildlife, and
/// loot all draw from one generator, adding a single draw to wildlife shifts every
/// subsequent weather value — quietly invalidating saved runs and repro seeds. Forked
/// streams are independent, so systems can evolve without disturbing each other.
/// </para>
/// </remarks>
public sealed class SimRng
{
    /// <summary>The golden-ratio increment used by splitmix64.</summary>
    private const ulong SplitMixGamma = 0x9E3779B97F4A7C15UL;

    /// <summary>xorshift64* output multiplier.</summary>
    private const ulong XorShiftMultiplier = 0x2545F4914F6CDD1DUL;

    /// <summary>
    /// Substituted whenever a scramble lands on zero. A zero xorshift state is absorbing —
    /// it would emit nothing but zeroes forever — so it must never be allowed to exist.
    /// </summary>
    private const ulong ZeroStateFallback = 0x853C49E6748FEA9BUL;

    private ulong _state;

    /// <summary>
    /// Creates a generator from a run seed.
    /// </summary>
    /// <param name="seed">
    /// The run seed. Sequential seeds (1, 2, 3…) are fine — they are scrambled through
    /// splitmix64 first, so they produce uncorrelated streams rather than neighbouring ones.
    /// </param>
    public SimRng(ulong seed)
    {
        _state = NonZero(SplitMix64(seed));
    }

    private SimRng(ulong state, bool _)
    {
        _state = state;
    }

    /// <summary>
    /// The raw generator state, captured into <c>SimState</c> on save. Never zero.
    /// </summary>
    public ulong State => _state;

    /// <summary>
    /// Restores a generator mid-stream from a previously captured <see cref="State"/>.
    /// </summary>
    /// <param name="state">A state captured from <see cref="State"/>.</param>
    /// <returns>A generator that continues exactly where the captured one left off.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="state"/> is zero, which no live generator can produce and
    /// which would yield a dead stream. That means a corrupt save — fail loudly rather than
    /// silently hand back a generator that only ever returns zero.
    /// </exception>
    public static SimRng FromState(ulong state)
    {
        if (state == 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(state),
                "A zero RNG state is invalid (the xorshift stream would be dead). The save is corrupt.");
        }

        return new SimRng(state, true);
    }

    /// <summary>
    /// Derives an independent child stream, <strong>without advancing this generator</strong>.
    /// </summary>
    /// <param name="streamId">
    /// A stable identifier for the consuming system. Use one fixed id per system (weather,
    /// wildlife, loot…) so its stream stays reproducible across versions.
    /// </param>
    /// <returns>A deterministic child generator independent of this one and of its siblings.</returns>
    /// <remarks>
    /// Not advancing the parent is the load-bearing property: it means the set of forks a
    /// run takes can grow — a new system added in M3 calling <c>Fork(9)</c> — without
    /// shifting any stream that already existed. Deriving by hashing rather than by drawing
    /// is what buys that.
    /// </remarks>
    public SimRng Fork(ulong streamId)
    {
        unchecked
        {
            var mixed = SplitMix64(_state ^ SplitMix64(streamId + SplitMixGamma));
            return new SimRng(NonZero(mixed), true);
        }
    }

    /// <summary>
    /// Draws the next 64-bit value and advances the stream.
    /// </summary>
    /// <returns>A pseudo-random 64-bit value.</returns>
    public ulong NextUInt64()
    {
        unchecked
        {
            _state ^= _state >> 12;
            _state ^= _state << 25;
            _state ^= _state >> 27;
            return _state * XorShiftMultiplier;
        }
    }

    /// <summary>
    /// Draws a value in <c>[0, 1)</c>.
    /// </summary>
    /// <returns>A pseudo-random double, never equal to 1.</returns>
    public double NextDouble()
    {
        // Take the top 53 bits — exactly the mantissa width of a double, so every
        // representable value in [0,1) is reachable and none is favoured.
        return (NextUInt64() >> 11) * (1.0 / (1UL << 53));
    }

    /// <summary>
    /// Draws an integer in <c>[minInclusive, maxExclusive)</c>.
    /// </summary>
    /// <param name="minInclusive">Inclusive lower bound.</param>
    /// <param name="maxExclusive">Exclusive upper bound.</param>
    /// <returns>
    /// A uniformly distributed integer in the half-open range, or
    /// <paramref name="minInclusive"/> if the range is empty (in which case the stream is
    /// not advanced).
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="maxExclusive"/> is less than <paramref name="minInclusive"/>.
    /// </exception>
    public int NextInt(int minInclusive, int maxExclusive)
    {
        if (maxExclusive < minInclusive)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxExclusive),
                $"maxExclusive ({maxExclusive}) must be greater than or equal to minInclusive ({minInclusive}).");
        }

        var range = (ulong)((long)maxExclusive - minInclusive);
        if (range == 0)
        {
            return minInclusive;
        }

        // Rejection sampling: plain modulo would over-represent the low end of the range
        // whenever range does not divide 2^64. Bias here would show up as loaded dice in
        // loot tables and weather spawns, so it is worth the occasional extra draw.
        var limit = ulong.MaxValue / range * range;
        ulong draw;
        do
        {
            draw = NextUInt64();
        }
        while (draw >= limit);

        return (int)((long)minInclusive + (long)(draw % range));
    }

    /// <summary>
    /// Draws a float in <c>[min, max)</c>.
    /// </summary>
    /// <param name="min">Inclusive lower bound.</param>
    /// <param name="max">Exclusive upper bound.</param>
    /// <returns>A uniformly distributed float in the range.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="max"/> is less than <paramref name="min"/>.
    /// </exception>
    public float NextFloat(float min, float max)
    {
        if (max < min)
        {
            throw new ArgumentOutOfRangeException(
                nameof(max),
                $"max ({max}) must be greater than or equal to min ({min}).");
        }

        return (float)(min + ((max - min) * NextDouble()));
    }

    /// <summary>
    /// splitmix64 — a strong 64-bit mixer. Used for seeding and forking rather than for
    /// output, so that seeds and stream ids with structure (0, 1, 2…) still yield
    /// well-separated states.
    /// </summary>
    private static ulong SplitMix64(ulong z)
    {
        unchecked
        {
            z += SplitMixGamma;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }
    }

    private static ulong NonZero(ulong value) => value == 0 ? ZeroStateFallback : value;
}
