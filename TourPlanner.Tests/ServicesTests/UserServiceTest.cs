using NUnit.Framework;
using NSubstitute;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using TourPlanner.Entities;
using TourPlanner.Services;
using TourPlanner.Repositories;

// Falls deine Custom Exceptions in einem eigenen Namespace liegen,
// musst du hier evtl. noch ein using hinzufügen (z.B. using TourPlanner.Exceptions;)

namespace TourPlanner.Tests.Services;

[TestFixture]
public class UserServiceTests
{
    private UserRepository _userRepoSub;
    private IConfiguration _configSub;
    private UserService _userService;

    [SetUp]
    public void Setup()
    {
        // Note: In order for Substitute.For to work with concrete classes, 
        // the methods in UserRepository must be marked as 'virtual'!
        _userRepoSub = Substitute.For<UserRepository>(new object[] { null });
        _configSub = Substitute.For<IConfiguration>();
        _userService = new UserService(_userRepoSub, _configSub);
    }

    // --- Tests for GetUserByUsername ---

    [Test]
    public async Task GetUserByUsername_UserExists_ReturnsUser()
    {
        // Arrange
        var user = new User { Id = 1, Username = "testuser" };
        _userRepoSub.GetUserByUsername("testuser").Returns(Task.FromResult(user));

        // Act
        var result = await _userService.GetUserByUsername("testuser");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Username, Is.EqualTo("testuser"));
    }

    [Test]
    public void GetUserByUsername_UserDoesNotExist_ThrowsUserNotFoundException()
    {
        // Arrange
        _userRepoSub.GetUserByUsername("unknown").Returns(Task.FromResult<User>(null));

        // Act & Assert
        // Assert.ThrowsAsync catches the exception and verifies that it is of the expected type
        Assert.ThrowsAsync<UserNotFoundException>(async () => 
            await _userService.GetUserByUsername("unknown"));
    }

    // --- Tests for RegisterUser ---

    [Test]
    public async Task RegisterUser_ValidData_ReturnsUserAndCallsAdd()
    {
        // Arrange
        var username = "newuser";
        var password = "password123";
        // Simulates that user DOES NOT exist (returns null)
        _userRepoSub.GetUserByUsername(username).Returns(Task.FromResult<User>(null)); 

        // Act
        var result = await _userService.RegisterUser(username, password);

        // Assert
        Assert.That(result.Username, Is.EqualTo(username));
        Assert.That(result.Password, Is.EqualTo(password));
        await _userRepoSub.Received(1).AddUser(Arg.Any<User>());
    }

    [Test]
    public void RegisterUser_UserAlreadyExists_ThrowsUserAlreadyExistsException()
    {
        // Arrange
        var existingUser = new User { Username = "duplicate" };
        // Simulates that user DOES exist
        _userRepoSub.GetUserByUsername("duplicate").Returns(Task.FromResult(existingUser)); 

        // Act & Assert
        Assert.ThrowsAsync<UserAlreadyExistsException>(async () => 
            await _userService.RegisterUser("duplicate", "pass123"));
    }

    // --- Tests für LoginUser ---

    [Test]
    public async Task LoginUser_ValidCredentials_ReturnsJwtToken()
    {
        // Arrange
        var username = "student";
        var password = "correctpassword";
        var userInDb = new User { Id = 1, Username = username, Password = password };
        
        _userRepoSub.GetUserByUsername(username).Returns(Task.FromResult(userInDb));

        // To make the token generation work, we need to mock the IConfiguration.
        // HmacSha256 strictly requires a key with at least 16 characters!
        _configSub["Jwt:Key"].Returns("mein_super_geheimer_test_key_1234567890"); 
        _configSub["Jwt:Issuer"].Returns("TestIssuer");
        _configSub["Jwt:Audience"].Returns("TestAudience");

        // Act
        var token = await _userService.LoginUser(username, password);

        // Assert
        Assert.That(token, Is.Not.Null);
        Assert.That(token, Is.Not.Empty);
    }

    [Test]
    public void LoginUser_WrongPassword_ThrowsException()
    {
        // Arrange
        var username = "student";
        var userInDb = new User { Id = 1, Username = username, Password = "correctpassword" };
        
        _userRepoSub.GetUserByUsername(username).Returns(Task.FromResult(userInDb));

        // Act & Assert
        Assert.ThrowsAsync<Exception>(async () => 
            await _userService.LoginUser(username, "wrongpassword"));
    }

    [Test]
    public void LoginUser_UserDoesNotExist_ThrowsException()
    {
        // Arrange
        _userRepoSub.GetUserByUsername("ghost").Returns(Task.FromResult<User>(null));

        // Act & Assert
        Assert.ThrowsAsync<Exception>(async () => 
            await _userService.LoginUser("ghost", "anypassword"));
    }
}