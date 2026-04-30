namespace MacroMaster.Domain.Models;

public sealed class PlaybackSettings
{
    public double SpeedMultiplier { get; set; } = 1.0;
    public int RepeatCount { get; set; } = 1;
    public int InitialDelayMs { get; set; } = 0;
    public bool LoopIndefinitely { get; set; } = false;
    public bool UseRelativeCoordinates { get; set; } = false;
    public bool StopOnError { get; set; } = true;
    public bool PreserveOriginalTiming { get; set; } = true;
}
