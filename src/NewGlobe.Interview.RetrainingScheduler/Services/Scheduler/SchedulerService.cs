using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NewGlobe.Interview.RetrainingScheduler.Models;

namespace NewGlobe.Interview.RetrainingScheduler.Services.Scheduler;

public class SchedulerService : ISchedulerService
{
    private readonly SchedulerConfiguration _configuration;
    private readonly ILogger<SchedulerService> _logger;
    private readonly IFileSystem _fileSystem;

    public SchedulerService(
        IOptions<SchedulerConfiguration> options,
        ILogger<SchedulerService> logger,
        IFileSystem fileSystem)
    {
        _configuration = options.Value;
        _logger = logger;
        _fileSystem = fileSystem;
    }
    
    public async Task ScheduleAsync()
    {
        var filePath = _configuration.InputFilePath;
        try
        {
            _logger.LogInformation("Reading input data from {path}", filePath);
            var input = await _fileSystem.File.ReadAllLinesAsync(filePath);

            if (input.Length == 0)
            {
                _logger.LogError("Input data is empty or invalid");
                return;
            }

            var sessions = ReadInputData(input);
            if (sessions.Count == 0)
            {
                _logger.LogWarning("No valid sessions found in the input data");
                return;
            }

            var tracks = ScheduleSessions(sessions);
            if (tracks.Count == 0)
            {
                _logger.LogWarning("No tracks were scheduled");
                return;
            }

            for (var i = 0; i < tracks.Count; i++)
            {
                var track = tracks[i];
                PrintTrack($"Track {i + 1}", track);
            }
        }
        catch (FileNotFoundException)
        {
            _logger.LogError("Input file could not be found on {path}", filePath);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the schedule");
            throw;
        }
    }

    private List<Session> ReadInputData(string[] lines)
    {
        List<Session> talks = new List<Session>();

        foreach (string line in lines)
        {
            string[] parts = line.Split('|').Select(p => p.Trim()).ToArray();
            if (parts.Length == 2)
            {
                string name = parts[0];
                string durationString = parts[1];
                int duration;

                if (durationString.EndsWith("min"))
                {
                    duration = int.Parse(durationString.Substring(0, durationString.Length - 3));
                }
                else if (durationString == "lightning")
                {
                    duration = 5; // Assuming lightning talks last for 5 minutes
                }
                else
                {
                    continue; // Skip invalid input
                }

                talks.Add(new Session { Name = name, Duration = duration });
            }
        }

        return talks;
    }

    
    private List<Track> ScheduleSessions(List<Session> sessions)
    {
        sessions = sessions.ToList();

        var tracks = new List<Track>();

        var track = new Track();
        tracks.Add(track);

        int morningTimeLimit = 180; // 9 AM to 12 PM (in minutes)
        int afternoonTimeLimit = 240; // 1 PM to 5 PM (in minutes)

        for (var i = 0; i < sessions.Count; i++)
        {
            var session = sessions[i];
            
            if (morningTimeLimit >= session.Duration)
            {
                track.MorningSession.Add(new Session
                {
                    Name = session.Name,
                    Duration = session.Duration
                });
                morningTimeLimit -= session.Duration;
            }
            else if (afternoonTimeLimit >= session.Duration)
            {
                track.AfternoonSession.Add(new Session
                {
                    Name = session.Name,
                    Duration = session.Duration
                });
                afternoonTimeLimit -= session.Duration;
            }
            else
            {
                track = new Track();
                tracks.Add(track);
                morningTimeLimit = 180;
                afternoonTimeLimit = 240;
                
                _logger.LogInformation("Added track #{trackNumber}", tracks.Count);
            }

            if (afternoonTimeLimit < session.Duration || i + 1 == sessions.Count)
            {
                AddSharingSession(track);
            }
        }

        return tracks;
    }

    private static void AddSharingSession(Track track)
    {
        track.AfternoonSession.Add(new Session
        {
            Name = "Sharing Session",
            Duration = 0
        });
    }

    private static void PrintTrack(string trackName, Track track)
    {
        Console.WriteLine($"{trackName}:");
        int morningStartTime = 9 * 60;
        foreach (var session in track.MorningSession)
        {
            string sessionTime = $"{morningStartTime / 60:D2}:{morningStartTime % 60:D2} AM";
            Console.WriteLine($"{sessionTime} | {session.Name} | {session.Duration}min");
            morningStartTime += session.Duration;
        }

        Console.WriteLine("12:00 PM | Lunch | 60min");

        int afternoonStartTime = 13 * 60;
        foreach (var session in track.AfternoonSession)
        {
            string sessionTime = $"{afternoonStartTime / 60:D2}:{afternoonStartTime % 60:D2} PM";
            Console.WriteLine($"{sessionTime} | {session.Name} | " +
                              (session.Duration > 0 ? $"{session.Duration}min" : string.Empty));
            afternoonStartTime += session.Duration;
        }

        Console.WriteLine();
    }
}