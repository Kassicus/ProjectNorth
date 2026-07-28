namespace ProjectNorth.Sim.Core;

/// <summary>
/// A 2D vector in world space, in sim units.
/// </summary>
/// <remarks>
/// This type exists for exactly one reason: so the simulation layer never needs
/// <c>Godot.Vector2</c> (CLAUDE.md rule 1). The Bridge layer inside <c>game/</c> converts
/// <see cref="SimVec2"/> ⇄ <c>Vector2</c> at the boundary, and nowhere else.
/// <para>
/// Deliberately minimal — this is a coordinate carrier, not a math library. Add operations
/// when a Sim system actually needs them, so the surface stays small enough to trust.
/// </para>
/// </remarks>
/// <param name="X">Horizontal component.</param>
/// <param name="Y">Vertical component.</param>
public readonly record struct SimVec2(float X, float Y)
{
    /// <summary>The origin, <c>(0, 0)</c>.</summary>
    public static SimVec2 Zero => new(0f, 0f);

    /// <summary>
    /// The squared magnitude. Prefer this over <see cref="Length"/> for comparisons
    /// and radius checks — weather-system overlap tests (TECH §4.2) run per sample,
    /// and skipping the square root is free accuracy as well as free speed.
    /// </summary>
    public float LengthSquared => (X * X) + (Y * Y);

    /// <summary>The Euclidean magnitude.</summary>
    public float Length => MathF.Sqrt(LengthSquared);

    /// <summary>Componentwise addition.</summary>
    /// <param name="a">Left operand.</param>
    /// <param name="b">Right operand.</param>
    /// <returns>The componentwise sum.</returns>
    public static SimVec2 operator +(SimVec2 a, SimVec2 b) => new(a.X + b.X, a.Y + b.Y);

    /// <summary>Componentwise subtraction.</summary>
    /// <param name="a">Left operand.</param>
    /// <param name="b">Right operand.</param>
    /// <returns>The componentwise difference.</returns>
    public static SimVec2 operator -(SimVec2 a, SimVec2 b) => new(a.X - b.X, a.Y - b.Y);

    /// <summary>Negation.</summary>
    /// <param name="v">The vector to negate.</param>
    /// <returns>The vector pointing the opposite way.</returns>
    public static SimVec2 operator -(SimVec2 v) => new(-v.X, -v.Y);

    /// <summary>Scalar multiplication.</summary>
    /// <param name="v">The vector.</param>
    /// <param name="scalar">The scale factor.</param>
    /// <returns>The scaled vector.</returns>
    public static SimVec2 operator *(SimVec2 v, float scalar) => new(v.X * scalar, v.Y * scalar);

    /// <summary>Scalar multiplication.</summary>
    /// <param name="scalar">The scale factor.</param>
    /// <param name="v">The vector.</param>
    /// <returns>The scaled vector.</returns>
    public static SimVec2 operator *(float scalar, SimVec2 v) => v * scalar;

    /// <inheritdoc />
    public override string ToString() => $"({X}, {Y})";
}
