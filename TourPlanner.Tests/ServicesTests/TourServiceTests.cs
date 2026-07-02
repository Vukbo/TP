using NUnit.Framework;
using NSubstitute;
using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using TourPlanner.Entities;
using TourPlanner.Services;
using TourPlanner.Repositories;
using TourPlanner.API.DTO;

namespace TourPlanner.Tests.Services;

[TestFixture]
public class TourServiceTests
{
    private TourRepository _tourRepoSub;
    private IMapper _mapperSub;
    private TourService _tourService;

    [SetUp]
    public void Setup()
    {
        // Note: In order for Substitute.For to work with concrete classes, 
        // the methods in UserRepository must be marked as 'virtual'!
        _tourRepoSub = Substitute.For<TourRepository>(new object[] { null });
        _mapperSub = Substitute.For<IMapper>();
        
        _tourService = new TourService(_tourRepoSub, _mapperSub);
    }

    [Test]
    public async Task AddTour_ValidData_SetsUserIdAndReturnsDto()
    {
        // Arrange
        int userId = 5;
        var inputDto = new TourDTO { Name = "Wanderung", From = "Wien", To = "Salzburg" };
        var mappedTour = new Tour { Name = "Wanderung", From = "Wien", To = "Salzburg", UserId = 5 };
        var savedTour = new Tour { Id = 1, Name = "Wanderung", From = "Wien", To = "Salzburg", UserId = 5 };
        var outputDto = new TourDTO { Id = 1, Name = "Wanderung", From = "Wien", To = "Salzburg" };

        _mapperSub.Map<Tour>(inputDto).Returns(mappedTour);
        _tourRepoSub.AddTour(mappedTour).Returns(Task.FromResult(savedTour));
        _mapperSub.Map<TourDTO>(savedTour).Returns(outputDto);

        // Act
        var result = await _tourService.AddTour(userId, inputDto);

        // Assert
        Assert.That(inputDto.UserId, Is.EqualTo(5)); 
        Assert.That(result.Id, Is.EqualTo(1));
        await _tourRepoSub.Received(1).AddTour(mappedTour);
    }

    [Test]
    public async Task GetTour_ExistingTour_ReturnsMappedDto()
    {
        // Arrange
        int userId = 2;
        int tourId = 10;
        var tourFromDb = new Tour { Id = tourId, UserId = userId, Name = "Tour 1", From = "A", To = "B" };
        var expectedDto = new TourDTO { Id = tourId, Name = "Tour 1", From = "A", To = "B" };

        _tourRepoSub.GetTourById(userId, tourId).Returns(Task.FromResult(tourFromDb));
        _mapperSub.Map<TourDTO>(tourFromDb).Returns(expectedDto);

        // Act
        var result = await _tourService.GetTour(tourId, userId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(10));
    }

    [Test]
    public async Task GetTours_ReturnsListOfTours()
    {
        // Arrange
        int userId = 1;
        var toursFromDb = new List<Tour> 
        { 
            new Tour { Name = "T1", From = "A", To = "B" }, 
            new Tour { Name = "T2", From = "C", To = "D" } 
        };
        var expectedDtos = new List<TourDTO> 
        { 
            new TourDTO { Name = "T1", From = "A", To = "B" }, 
            new TourDTO { Name = "T2", From = "C", To = "D" } 
        };

        _tourRepoSub.GetAllTours(userId).Returns(Task.FromResult(toursFromDb));
        _mapperSub.Map<List<TourDTO>>(toursFromDb).Returns(expectedDtos);

        // Act
        var result = await _tourService.GetTours(userId);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task ExportTours_ReturnsListOfTours()
    {
        // Arrange
        int userId = 1;
        var toursFromDb = new List<Tour> { new Tour { Name = "Export", From = "A", To = "B" } };
        var expectedDtos = new List<TourDTO> { new TourDTO { Name = "Export", From = "A", To = "B" } };

        _tourRepoSub.GetAllTours(userId).Returns(Task.FromResult(toursFromDb));
        _mapperSub.Map<List<TourDTO>>(toursFromDb).Returns(expectedDtos);

        // Act
        var result = await _tourService.ExportTours(userId);

        // Assert
        Assert.That(result.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task UpdateTour_ValidData_SetsIdsAndReturnsDto()
    {
        // Arrange
        int userId = 3;
        int tourId = 7;
        var inputDto = new TourDTO { Name = "Update", From = "Graz", To = "Linz" };
        var mappedTour = new Tour { Id = 7, UserId = 3, Name = "Update", From = "Graz", To = "Linz" };
        var outputDto = new TourDTO { Id = 7, Name = "Update", From = "Graz", To = "Linz" };

        _mapperSub.Map<Tour>(inputDto).Returns(mappedTour);
        _tourRepoSub.UpdateTour(mappedTour).Returns(Task.FromResult(mappedTour));
        _mapperSub.Map<TourDTO>(mappedTour).Returns(outputDto);

        // Act
        var result = await _tourService.UpdateTour(inputDto, tourId, userId);

        // Assert
        Assert.That(inputDto.Id, Is.EqualTo(7));
        Assert.That(inputDto.UserId, Is.EqualTo(3));
        Assert.That(result.Id, Is.EqualTo(7));
        await _tourRepoSub.Received(1).UpdateTour(mappedTour);
    }

    [Test]
    public async Task DeleteTour_CallsRepositoryDelete()
    {
        // Arrange
        int tourId = 4;
        int userId = 1;

        // Act
        await _tourService.DeleteTour(tourId, userId);

        // Assert
        await _tourRepoSub.Received(1).DeleteTour(tourId, userId);
    }

    [Test]
    public async Task ImportTours_ValidList_SetsIdsToNullAndCallsAdd()
    {
        // Arrange
        int userId = 5;
        var importList = new List<TourDTO> 
        { 
            new TourDTO { Id = 99, Name = "Import 1", From = "A", To = "B" }, 
            new TourDTO { Id = 100, Name = "Import 2", From = "C", To = "D" }
        };
        
        var mappedTour = new Tour { Name = "Mapped", From = "X", To = "Y" };
        
        _mapperSub.Map<Tour>(Arg.Any<TourDTO>()).Returns(mappedTour);

        // Act
        await _tourService.ImportTours(importList, userId);

        // Assert
        Assert.That(importList[0].Id, Is.Null);
        Assert.That(importList[0].UserId, Is.EqualTo(5));
        
        Assert.That(importList[1].Id, Is.Null);
        Assert.That(importList[1].UserId, Is.EqualTo(5));

        await _tourRepoSub.Received(2).AddTour(Arg.Any<Tour>());
    }
    
    [Test]
    public async Task GetTour_NonExistingId_ReturnsNull()
    {
        // Arrange
        int userId = 1;
        int nonExistingTourId = 999;
        
        _tourRepoSub.GetTourById(userId, nonExistingTourId).Returns(Task.FromResult<Tour>(null));
        _mapperSub.Map<TourDTO>(null).Returns((TourDTO)null);

        // Act
        var result = await _tourService.GetTour(nonExistingTourId, userId);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetTours_NoToursExist_ReturnsEmptyList()
    {
        // Arrange
        int userId = 1;
        var emptyListFromDb = new List<Tour>();
        var emptyDtoList = new List<TourDTO>();

        _tourRepoSub.GetAllTours(userId).Returns(Task.FromResult(emptyListFromDb));
        _mapperSub.Map<List<TourDTO>>(emptyListFromDb).Returns(emptyDtoList);

        // Act
        var result = await _tourService.GetTours(userId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }
}