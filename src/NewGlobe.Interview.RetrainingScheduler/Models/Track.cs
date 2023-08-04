namespace NewGlobe.Interview.RetrainingScheduler.Models;

public class Track
{
    public List<Session> MorningSession { get; set; } = new List<Session>();
    public List<Session> AfternoonSession { get; set; } = new List<Session>();
}