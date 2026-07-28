using ProjectNorth.Sim.Core;

namespace ProjectNorth.Sim.Tests.Core;

/// <summary>
/// CLAUDE.md rule 2: a seed must fully determine a run. Everything downstream —
/// forecastable weather (TECH §4.2), the balance harness (TECH §6), reproducible
/// bug reports — rests on the assertions in this file.
/// </summary>
public class SimRngTests
{
    private static ulong[] Draw(SimRng rng, int count)
    {
        var values = new ulong[count];
        for (var i = 0; i < count; i++)
        {
            values[i] = rng.NextUInt64();
        }

        return values;
    }

    [Fact]
    public void SameSeed_ProducesIdenticalSequence()
    {
        Assert.Equal(Draw(new SimRng(12345), 100), Draw(new SimRng(12345), 100));
    }

    [Fact]
    public void DifferentSeeds_Diverge()
    {
        Assert.NotEqual(Draw(new SimRng(1), 100), Draw(new SimRng(2), 100));
    }

    /// <summary>
    /// The splitmix64 scramble earns its keep here: raw xorshift seeded with 1, 2, 3
    /// would produce visibly correlated streams. Sequential seeds are exactly what a
    /// human types, so they have to behave.
    /// </summary>
    [Fact]
    public void SequentialSeeds_ProduceUncorrelatedStreams()
    {
        var firstDraws = new List<ulong>();
        for (ulong seed = 1; seed <= 64; seed++)
        {
            firstDraws.Add(new SimRng(seed).NextUInt64());
        }

        Assert.Equal(firstDraws.Count, firstDraws.Distinct().Count());

        // A correlated generator leaks the seed into the low bits of its first draw.
        Assert.True(firstDraws.Select(v => v & 0xFF).Distinct().Count() > 40);
    }

    [Fact]
    public void SeedZero_StillProducesAUsefulStream()
    {
        // xorshift state must never be zero, or the generator is dead forever.
        var draws = Draw(new SimRng(0), 50);
        Assert.DoesNotContain(0UL, draws);
        Assert.True(draws.Distinct().Count() > 45);
    }

    [Fact]
    public void NextDouble_StaysInUnitInterval()
    {
        var rng = new SimRng(7);
        for (var i = 0; i < 10_000; i++)
        {
            var value = rng.NextDouble();
            Assert.InRange(value, 0.0, 1.0);
            Assert.NotEqual(1.0, value);
        }
    }

    [Fact]
    public void NextInt_RespectsBoundsOverManyDraws()
    {
        var rng = new SimRng(99);
        var seen = new HashSet<int>();
        for (var i = 0; i < 20_000; i++)
        {
            var value = rng.NextInt(-5, 5);
            Assert.InRange(value, -5, 4);
            seen.Add(value);
        }

        // Every value in the half-open range should appear; none outside it.
        Assert.Equal(10, seen.Count);
    }

    [Fact]
    public void NextInt_WithEmptyRange_ReturnsTheLowerBound()
    {
        var rng = new SimRng(3);
        Assert.Equal(4, rng.NextInt(4, 4));
    }

    [Fact]
    public void NextInt_WithInvertedRange_Throws()
    {
        var rng = new SimRng(3);
        Assert.Throws<ArgumentOutOfRangeException>(() => rng.NextInt(5, 4));
    }

    [Fact]
    public void NextFloat_StaysWithinRange()
    {
        var rng = new SimRng(4242);
        for (var i = 0; i < 10_000; i++)
        {
            Assert.InRange(rng.NextFloat(-12.5f, 30f), -12.5f, 30f);
        }
    }

    [Fact]
    public void Fork_DoesNotAdvanceTheParent()
    {
        var parent = new SimRng(2024);
        parent.NextUInt64();

        var stateBefore = parent.State;
        _ = parent.Fork(1);
        _ = parent.Fork(2);
        _ = parent.Fork(999);

        Assert.Equal(stateBefore, parent.State);
    }

    [Fact]
    public void Fork_IsDeterministicForAGivenParentStateAndStream()
    {
        var a = new SimRng(2024).Fork(7);
        var b = new SimRng(2024).Fork(7);

        Assert.Equal(Draw(a, 50), Draw(b, 50));
    }

    [Fact]
    public void Fork_WithDifferentStreamIds_DivergesFromEachOtherAndTheParent()
    {
        var parent = new SimRng(2024);
        var weather = parent.Fork(1);
        var wildlife = parent.Fork(2);

        var weatherDraws = Draw(weather, 50);
        var wildlifeDraws = Draw(wildlife, 50);
        var parentDraws = Draw(parent, 50);

        Assert.NotEqual(weatherDraws, wildlifeDraws);
        Assert.NotEqual(weatherDraws, parentDraws);
        Assert.NotEqual(wildlifeDraws, parentDraws);
    }

    /// <summary>
    /// The intended usage pattern (brief §1b): one child stream per system, so adding a
    /// draw to wildlife never shifts the weather sequence out from under a saved run.
    /// </summary>
    [Fact]
    public void Fork_IsolatesSystemsFromEachOthersDraws()
    {
        var master = new SimRng(555);
        var weatherBefore = Draw(master.Fork(1), 20);

        var wildlife = master.Fork(2);
        Draw(wildlife, 5_000);

        Assert.Equal(weatherBefore, Draw(master.Fork(1), 20));
    }

    [Fact]
    public void State_RoundTripsThroughFromState()
    {
        var original = new SimRng(31337);
        Draw(original, 17);

        var restored = SimRng.FromState(original.State);

        Assert.Equal(Draw(original, 100), Draw(restored, 100));
    }

    [Fact]
    public void FromState_RejectsZero()
    {
        // A zero state is a corrupt save, not a valid stream — fail loudly.
        Assert.Throws<ArgumentOutOfRangeException>(() => SimRng.FromState(0));
    }

    /// <summary>
    /// Pins the exact byte stream for seed 1. If a refactor changes the algorithm this
    /// test fails — which is the point: every existing save and every recorded repro
    /// seed would silently mean something different.
    /// </summary>
    [Fact]
    public void Algorithm_IsPinnedAgainstAccidentalChange()
    {
        var expected = new ulong[]
        {
            5424204624148110235,
            15555979849632202484,
            6851360858507811590,
            4263911567865507035,
            15846549526847483984,
        };

        Assert.Equal(expected, Draw(new SimRng(1), 5));
    }
}
