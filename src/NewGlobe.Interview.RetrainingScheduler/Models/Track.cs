namespace NewGlobe.Interview.RetrainingScheduler.Models;

public class Track
{
    public List<Session> MorningSession { get; } = new List<Session>();
    public List<Session> AfternoonSession { get; } = new List<Session>();
}