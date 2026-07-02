using NUnit.Framework;
using NSubstitute;
using AutoMapper;
using System.Threading.Tasks;
using TourPlanner.Entities;
using TourPlanner.Services;
using TourPlanner.DAL;
using TourPlanner.API.DTO;

namespace TourPlanner.Tests.Services;

[TestFixture]
public class TourLogServiceTests
{
    private TourLogRepository _logRepoSub;
    private IMapper _mapperSub;
    private TourLogService _logService;

    [SetUp]
    public void Setup()
    {
        // Note: In order for Substitute.For to work with concrete classes, 
        // // the methods in UserRepository must be marked as 'virtual'!
        _logRepoSub = Substitute.For<TourLogRepository>(new object[] { null });
        _mapperSub = Substitute.For<IMapper>();
        
        _logService = new TourLogService(_logRepoSub, _mapperSub);
    }

    [Test]
    public async Task AddTourLog_ValidData_CallsAddAndReturnsMappedDto()
    {
        // Arrange
        var inputDto = new TourLogDTO { TourId = 1, Comment = "Schöne Strecke", Distance = 15.5f };
        var mappedLog = new TourLog { TourId = 1, Comment = "Schöne Strecke", Distance = 15.5f };
        var savedLog = new TourLog { Id = 10, TourId = 1, Comment = "Schöne Strecke", Distance = 15.5f };
        var outputDto = new TourLogDTO { Id = 10, TourId = 1, Comment = "Schöne Strecke", Distance = 15.5f };

        _mapperSub.Map<TourLog>(inputDto).Returns(mappedLog);
        _logRepoSub.AddTourLog(mappedLog).Returns(Task.FromResult(savedLog));
        _mapperSub.Map<TourLogDTO>(savedLog).Returns(outputDto);

        // Act
        var result = await _logService.AddTourLog(inputDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(10));
        Assert.That(result.Comment, Is.EqualTo("Schöne Strecke"));
        
        // Check if the repository method was called exactly once with the mapped log
        await _logRepoSub.Received(1).AddTourLog(mappedLog);
    }

    [Test]
    public async Task UpdateTourLog_ValidData_SetsIdAndCallsUpdate()
    {
        // Arrange
        int tourLogId = 5;
        var inputDto = new TourLogDTO { TourId = 2, Comment = "Update gemacht" };
        var mappedLog = new TourLog { Id = 5, TourId = 2, Comment = "Update gemacht" };
        var outputDto = new TourLogDTO { Id = 5, TourId = 2, Comment = "Update gemacht" };

        _mapperSub.Map<TourLog>(inputDto).Returns(mappedLog);
        _logRepoSub.UpdateTourLog(mappedLog).Returns(Task.FromResult(mappedLog));
        _mapperSub.Map<TourLogDTO>(mappedLog).Returns(outputDto);

        // Act
        var result = await _logService.UpdateTourLog(inputDto, tourLogId);

        // Assert
        Assert.That(inputDto.Id, Is.EqualTo(5)); 
        Assert.That(result.Id, Is.EqualTo(5));
        
        await _logRepoSub.Received(1).UpdateTourLog(mappedLog);
    }

    [Test]
    public async Task DeleteTourLog_CallsRepositoryDeleteWithCorrectIds()
    {
        // Arrange
        int tourLogId = 42;
        var deleteDto = new TourLogDeleteDTO { TourId = 7 };

        // Act
        await _logService.DeleteTourLog(tourLogId, deleteDto);

        // Assert
        await _logRepoSub.Received(1).DeleteTourLog(tourLogId, 7);
    }
    
    
    [Test]
    public async Task UpdateTourLog_AlwaysOverwritesDtoIdWithParameterId()
    {
        // Arrange
        int parameterId = 99; // The correct ID passed to the URL/method
        
        // Someone maliciously tries to send a different ID (1) in the payload
        var inputDto = new TourLogDTO { Id = 1, TourId = 5, Comment = "Test" }; 
        var mappedLog = new TourLog { Id = 99, TourId = 5 };
        var outputDto = new TourLogDTO { Id = 99, TourId = 5 };

        _mapperSub.Map<TourLog>(inputDto).Returns(mappedLog);
        _logRepoSub.UpdateTourLog(mappedLog).Returns(Task.FromResult(mappedLog));
        _mapperSub.Map<TourLogDTO>(mappedLog).Returns(outputDto);

        // Act
        await _logService.UpdateTourLog(inputDto, parameterId);

        // Assert
        // Verifies that the service prevents the manipulation attempt 
        // and strictly overwrites the DTO's ID with the parameter ID
        Assert.That(inputDto.Id, Is.EqualTo(parameterId)); 
    }
}