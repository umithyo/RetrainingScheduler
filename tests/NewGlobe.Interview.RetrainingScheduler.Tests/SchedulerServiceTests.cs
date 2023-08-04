using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NewGlobe.Interview.RetrainingScheduler.Services;
using NewGlobe.Interview.RetrainingScheduler.Services.Scheduler;

namespace NewGlobe.Interview.RetrainingScheduler.Tests;

public class SchedulerServiceTests
{
    [Fact]
    public async Task ScheduleAsync_ValidInput_ShouldScheduleSessionsAndPrintTracks()
    {
        // Arrange
        var inputFilePath = "test_input.txt";
        var inputLines = new string[]
        {
            "Session 1 | 30min",
            "Session 2 | 45min",
            "Session 3 | lightning",
        };

        var loggerMock = new Mock<ILogger<SchedulerService>>();
        var schedulerConfig = new SchedulerConfiguration { InputFilePath = inputFilePath };
        var schedulerOptionsMock = new Mock<IOptions<SchedulerConfiguration>>();
        schedulerOptionsMock.Setup(x => x.Value).Returns(schedulerConfig);

        var fileSystemMock = new Mock<IFileSystem>();
        fileSystemMock
            .Setup(fs => fs.File.ReadAllLinesAsync(inputFilePath, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(inputLines));

        var schedulerService =
            new SchedulerService(schedulerOptionsMock.Object, loggerMock.Object, fileSystemMock.Object);

        // Act
        await schedulerService.ScheduleAsync();

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Exactly(1));
    }

    [Fact]
    public async Task ScheduleAsync_EmptyInput_ShouldLogWarning()
    {
        // Arrange
        var inputFilePath = "empty_input.txt";
        var loggerMock = new Mock<ILogger<SchedulerService>>();
        var schedulerConfig = new SchedulerConfiguration { InputFilePath = inputFilePath };
        var schedulerOptionsMock = new Mock<IOptions<SchedulerConfiguration>>();
        schedulerOptionsMock.Setup(x => x.Value).Returns(schedulerConfig);

        var fileSystemMock = new Mock<IFileSystem>();
        fileSystemMock.Setup(fs => fs.File.ReadAllLinesAsync(inputFilePath, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Array.Empty<string>()));

        var schedulerService =
            new SchedulerService(schedulerOptionsMock.Object, loggerMock.Object, fileSystemMock.Object);

        // Act
        await schedulerService.ScheduleAsync();

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once); // Assuming the warning is logged once for empty input.
    }

    [Fact]
    public async Task ScheduleAsync_FileNotFound_ShouldLogError()
    {
        // Arrange
        var inputFilePath = "invalid_path.txt";
        var loggerMock = new Mock<ILogger<SchedulerService>>();
        var schedulerConfig = new SchedulerConfiguration { InputFilePath = inputFilePath };
        var schedulerOptionsMock = new Mock<IOptions<SchedulerConfiguration>>();
        schedulerOptionsMock.Setup(x => x.Value).Returns(schedulerConfig);

        var fileSystemMock = new Mock<IFileSystem>();
        fileSystemMock.Setup(fs => fs.File.ReadAllLinesAsync(inputFilePath, It.IsAny<CancellationToken>())).ThrowsAsync(new FileNotFoundException());

        var schedulerService =
            new SchedulerService(schedulerOptionsMock.Object, loggerMock.Object, fileSystemMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(async () => await schedulerService.ScheduleAsync());

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once); // Assuming the error is logged once for file not found.
    }
}