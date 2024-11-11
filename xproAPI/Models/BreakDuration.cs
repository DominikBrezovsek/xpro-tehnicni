namespace xproAPI.Models;

public class BreakDuration
{
    public long Id { get; set; }
    public int Duration { get; set; }
    public bool Valid { get; set; }

    public ICollection<WorkTime> WorkTimes { get; } = new List<WorkTime>();
}