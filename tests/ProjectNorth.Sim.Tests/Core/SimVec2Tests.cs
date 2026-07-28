using ProjectNorth.Sim.Core;

namespace ProjectNorth.Sim.Tests.Core;

public class SimVec2Tests
{
    [Fact]
    public void Zero_IsOrigin()
    {
        Assert.Equal(0f, SimVec2.Zero.X);
        Assert.Equal(0f, SimVec2.Zero.Y);
    }

    [Fact]
    public void LengthSquared_AvoidsTheSquareRoot()
    {
        var v = new SimVec2(3f, 4f);
        Assert.Equal(25f, v.LengthSquared);
    }

    [Fact]
    public void Length_IsEuclidean()
    {
        var v = new SimVec2(3f, 4f);
        Assert.Equal(5f, v.Length, 5);
    }

    [Fact]
    public void Length_OfZero_IsZero()
    {
        Assert.Equal(0f, SimVec2.Zero.Length);
    }

    [Fact]
    public void Addition_IsComponentwise()
    {
        var sum = new SimVec2(1f, 2f) + new SimVec2(10f, 20f);
        Assert.Equal(new SimVec2(11f, 22f), sum);
    }

    [Fact]
    public void Subtraction_IsComponentwise()
    {
        var diff = new SimVec2(10f, 20f) - new SimVec2(1f, 2f);
        Assert.Equal(new SimVec2(9f, 18f), diff);
    }

    [Fact]
    public void ScalarMultiplication_WorksFromEitherSide()
    {
        var v = new SimVec2(2f, -3f);
        Assert.Equal(new SimVec2(6f, -9f), v * 3f);
        Assert.Equal(new SimVec2(6f, -9f), 3f * v);
    }

    [Fact]
    public void Negation_FlipsBothComponents()
    {
        Assert.Equal(new SimVec2(-2f, 3f), -new SimVec2(2f, -3f));
    }

    [Fact]
    public void ValueEquality_ComesFromRecordStruct()
    {
        Assert.Equal(new SimVec2(1.5f, 2.5f), new SimVec2(1.5f, 2.5f));
        Assert.NotEqual(new SimVec2(1.5f, 2.5f), new SimVec2(2.5f, 1.5f));
    }

    /// <summary>
    /// CLAUDE.md rule 1 in test form: SimVec2 exists so Sim never needs Godot.Vector2.
    /// If this type ever grows an engine dependency, this assembly stops compiling —
    /// but the assertion documents the intent for anyone reading the suite.
    /// </summary>
    [Fact]
    public void Type_LivesInASimAssemblyWithNoGodotDependency()
    {
        var assembly = typeof(SimVec2).Assembly;
        Assert.DoesNotContain(
            assembly.GetReferencedAssemblies(),
            a => a.Name?.Contains("Godot", StringComparison.OrdinalIgnoreCase) == true);
    }
}
