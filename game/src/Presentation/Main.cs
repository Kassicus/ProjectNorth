using Godot;
using ProjectNorth.Sim;
using ProjectNorth.Sim.Calendar;

namespace ProjectNorth.Game.Presentation;

/// <summary>
/// The M0 walking skeleton: drives the sim clock from engine frames and puts the date,
/// time, and actual season on screen.
/// </summary>
/// <remarks>
/// <para>
/// This is the only sanctioned shape for Sim ⇄ Godot traffic (CLAUDE.md rule 6), and every
/// controller added later should look like it:
/// </para>
/// <list type="number">
///   <item><description>Own a <see cref="SimWorld"/>; never construct Sim services directly.</description></item>
///   <item><description>Issue commands into Sim (here, <c>Clock.Advance</c>).</description></item>
///   <item><description>Read Sim state back out, and subscribe to Sim events for the rest.</description></item>
/// </list>
/// <para>
/// Nothing flows the other way: Sim has no idea a renderer exists.
/// </para>
/// </remarks>
public partial class Main : Node2D
{
    private SimWorld _world = null!;
    private Label _clockLabel = null!;

    /// <summary>
    /// Carries the sub-minute remainder between frames so time does not drift.
    /// </summary>
    /// <remarks>
    /// At 60 sim-minutes per real second a frame is worth about one minute, but never
    /// exactly. Truncating each frame's fraction would lose a slice of every frame — minutes
    /// a day at speed — and the sim clock is the one thing in the game that must not
    /// silently run slow.
    /// </remarks>
    private double _pendingMinutes;

    /// <summary>How many sim minutes pass per real second. 60 means one hour a second.</summary>
    [Export]
    public float SimMinutesPerRealSecond { get; set; } = 60f;

    /// <summary>
    /// The run seed. Fully determines the run (CLAUDE.md rule 2). Exported so a specific
    /// seed can be replayed from the editor without a code change.
    /// </summary>
    [Export]
    public long Seed { get; set; } = 4471;

    /// <inheritdoc />
    public override void _Ready()
    {
        _world = new SimWorld((ulong)Seed);
        _clockLabel = GetNode<Label>("ClockLabel");

        // The plane's cadence (GDD §4). In M1 this is where the order sheet arrives; for now
        // it proves the boundary events reach Presentation at the right moments.
        _world.Clock.WeekEnded += OnWeekEnded;

        // Fires on what is actually outside, not on the calendar page turning — which is
        // why the False Thaw can announce Spring -> Winter (CLAUDE.md rule 3).
        _world.Seasons.SeasonChanged += OnSeasonChanged;

        UpdateClockLabel();
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _pendingMinutes += delta * SimMinutesPerRealSecond;

        // Advance only whole minutes; keep the remainder for next frame.
        var wholeMinutes = (int)_pendingMinutes;
        if (wholeMinutes > 0)
        {
            _pendingMinutes -= wholeMinutes;
            _world.Clock.Advance(wholeMinutes);
        }

        UpdateClockLabel();
    }

    private static void OnWeekEnded(GameDate date) => GD.Print($"[plane day] {date}");

    private static void OnSeasonChanged(Season from, Season to) => GD.Print($"[season] {from} -> {to}");

    /// <summary>
    /// Renders the calendar/actual split on screen from day one: the date is what the wall
    /// calendar says, "outside" is what the world is actually doing. In Act 3 these stop
    /// agreeing, and it should be visible the moment they do.
    /// </summary>
    private void UpdateClockLabel()
    {
        var date = _world.Clock.CurrentDate;
        var minuteOfDay = _world.Clock.MinuteOfDay;

        _clockLabel.Text =
            $"{date}  {minuteOfDay / 60:D2}:{minuteOfDay % 60:D2}  (outside: {_world.Seasons.ActualSeason})";
    }
}
