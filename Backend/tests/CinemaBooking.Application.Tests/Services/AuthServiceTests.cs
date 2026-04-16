using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CinemaBooking.Application.DTOs.Auth;
using CinemaBooking.Application.Tests.Fixtures;
using CinemaBooking.Application.Tests.Helpers;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Enums;
using CinemaBooking.Infrastructure.Data;
using CinemaBooking.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;

namespace CinemaBooking.Application.Tests.Services;

/// <summary>
/// Unit Tests cho AuthService.
/// Bao phủ Register, Login, GenerateJwtToken, và EmailExistsAsync.
/// Pattern: AAA (Arrange, Act, Assert)
/// Sử dụng: Real in-memory DbContext + Mock IConfiguration
/// </summary>
[TestFixture]
public class AuthServiceTests
{
    private ApplicationDbContext _dbContext;
    private Mock<IConfiguration> _mockConfiguration;
    private AuthService _authService;

    [SetUp]
    public void Setup()
    {
        // Reset TestDataFactory để tránh ID conflicts giữa các tests
        TestDataFactory.Reset();
        
        // Tạo real in-memory DbContext
        _dbContext = TestDbContextBuilder.CreateInMemoryDbContext();
        
        // Setup mock IConfiguration với JWT settings
        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(c => c["Jwt:Secret"])
            .Returns("this-is-a-very-long-secret-key-for-jwt-testing-purposes-32");
        _mockConfiguration.Setup(c => c["Jwt:Issuer"])
            .Returns("CinemaBookingAPI");
        _mockConfiguration.Setup(c => c["Jwt:Audience"])
            .Returns("CinemaBookingClients");
        _mockConfiguration.Setup(c => c["Jwt:ExpiryMinutes"])
            .Returns("60");

        _authService = new AuthService(_dbContext, _mockConfiguration.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext?.Dispose();
    }

    #region Register Tests

    [Test]
    [Description("Đăng ký với dữ liệu hợp lệ. Kiểm tra user được tạo thành công.")]
    public async Task RegisterAsync_ValidData_ReturnsSuccess()
    {
        // ========== ARRANGE ==========
        var request = new RegisterRequestDto
        {
            Username = "newuser",
            Email = "newuser@example.com",
            Password = "ValidPass123",
            ConfirmPassword = "ValidPass123",
            FullName = "New User",
            PhoneNumber = "0912345678"
        };

        // ========== ACT ==========
        var result = await _authService.RegisterAsync(request);

        // ========== ASSERT ==========
        result.Should().NotBeNull();
        result.Username.Should().Be(request.Username);
        result.Email.Should().Be(request.Email);
        result.FullName.Should().Be(request.FullName);
        result.Role.Should().Be(UserRole.Customer.ToString());
        result.Token.Should().NotBeNullOrEmpty();
        result.TokenType.Should().Be("Bearer");

        // Verify user was saved to database
        var savedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        savedUser.Should().NotBeNull();
        savedUser.Email.Should().Be(request.Email);
    }

    [Test]
    [Description("Đăng ký với Username rỗng. Kiểm tra ném InvalidOperationException.")]
    public async Task RegisterAsync_UsernameEmpty_ThrowsException()
    {
        // ========== ARRANGE ==========
        var request = new RegisterRequestDto
        {
            Username = "",  // Empty
            Email = "user@example.com",
            Password = "ValidPass123",
            ConfirmPassword = "ValidPass123",
            FullName = "User Name",
            PhoneNumber = null
        };

        // ========== ACT & ASSERT ==========
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _authService.RegisterAsync(request));

        ex.Message.Should().Contain("Username");
        ex.Message.Should().Contain("không thể trống");
    }

    [Test]
    [Description("Đăng ký với Username null. Kiểm tra ném InvalidOperationException.")]
    public async Task RegisterAsync_UsernameNull_ThrowsException()
    {
        // ========== ARRANGE ==========
        var request = new RegisterRequestDto
        {
            Username = null,  // Null
            Email = "user@example.com",
            Password = "ValidPass123",
            ConfirmPassword = "ValidPass123",
            FullName = "User Name",
            PhoneNumber = null
        };

        // ========== ACT & ASSERT ==========
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _authService.RegisterAsync(request));

        ex.Message.Should().Contain("Username");
        ex.Message.Should().Contain("không thể trống");
    }

    [Test]
    [Description("Đăng ký với Username dưới 3 ký tự. Kiểm tra ném validation exception.")]
    public async Task RegisterAsync_UsernameTooShort_ThrowsException()
    {
        // ========== ARRANGE ==========
        var request = new RegisterRequestDto
        {
            Username = "ab",  // Less than 3 characters
            Email = "user@example.com",
            Password = "ValidPass123",
            ConfirmPassword = "ValidPass123",
            FullName = "User Name",
            PhoneNumber = null
        };

        // ========== ACT & ASSERT ==========
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _authService.RegisterAsync(request));

        ex.Message.Should().Contain("Tên đăng nhập");
        ex.Message.Should().Contain("3 đến 50 ký tự");
    }

    [Test]
    [Description("Đăng ký khi Username đã tồn tại. Kiểm tra ném InvalidOperationException.")]
    public async Task RegisterAsync_UsernameAlreadyExists_ThrowsException()
    {
        // ========== ARRANGE ==========
        var existingUser = UserFixture.CreateValidUser(username: "existinguser");
        _dbContext.Users.Add(existingUser);
        await _dbContext.SaveChangesAsync();

        var request = new RegisterRequestDto
        {
            Username = "existinguser",  // Already exists
            Email = "newuser@example.com",
            Password = "ValidPass123",
            ConfirmPassword = "ValidPass123",
            FullName = "New User",
            PhoneNumber = null
        };

        // ========== ACT & ASSERT ==========
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _authService.RegisterAsync(request));

        ex.Message.Should().Contain("existinguser");
        ex.Message.Should().Contain("đã được sử dụng");
    }

    [Test]
    [Description("Đăng ký khi Email đã tồn tại. Kiểm tra ném InvalidOperationException.")]
    public async Task RegisterAsync_EmailAlreadyExists_ThrowsException()
    {
        // ========== ARRANGE ==========
        var existingUser = UserFixture.CreateValidUser(email: "existing@example.com");
        _dbContext.Users.Add(existingUser);
        await _dbContext.SaveChangesAsync();

        var request = new RegisterRequestDto
        {
            Username = "newuser",
            Email = "existing@example.com",  // Already exists
            Password = "ValidPass123",
            ConfirmPassword = "ValidPass123",
            FullName = "New User",
            PhoneNumber = null
        };

        // ========== ACT & ASSERT ==========
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _authService.RegisterAsync(request));

        ex.Message.Should().Contain("Email");
        ex.Message.Should().Contain("existing@example.com");
        ex.Message.Should().Contain("đã được sử dụng");
    }

    #endregion

    #region Login Tests

    [Test]
    [Description("Đăng nhập với email và password hợp lệ. Kiểm tra trả về AuthResponseDto với Username.")]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        // ========== ARRANGE ==========
        const string password = "ValidPass123";
        const string username = "validuser";
        var user = UserFixture.CreateValidUser(username: username, password: password);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        
        // Clear tracker để tránh conflict khi AuthService.LoginAsync gọi Update
        _dbContext.ChangeTracker.Clear();

        // ========== ACT ==========
        var result = await _authService.LoginAsync(user.Email, password);

        // ========== ASSERT ==========
        result.Should().NotBeNull();
        result.Username.Should().Be(username);
        result.Email.Should().Be(user.Email);
        result.UserId.Should().Be(user.Id);
        result.Token.Should().NotBeNullOrEmpty();
        result.TokenType.Should().Be("Bearer");
        result.ExpiresAt.Should().BeGreaterThan(0);
    }

    [Test]
    [Description("Đăng nhập với email không tồn tại. Kiểm tra trả về null.")]
    public async Task LoginAsync_UserNotFound_ReturnsNull()
    {
        // ========== ARRANGE ==========
        const string email = "notexist@example.com";
        const string password = "ValidPass123";

        // ========== ACT ==========
        var result = await _authService.LoginAsync(email, password);

        // ========== ASSERT ==========
        result.Should().BeNull();
    }

    [Test]
    [Description("Đăng nhập với mật khẩu sai. Kiểm tra trả về null.")]
    public async Task LoginAsync_InvalidPassword_ReturnsNull()
    {
        // ========== ARRANGE ==========
        const string correctPassword = "ValidPass123";
        const string wrongPassword = "WrongPassword";
        var user = UserFixture.CreateValidUser(password: correctPassword);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        
        // Clear tracker để tránh conflict khi AuthService.LoginAsync gọi Update
        _dbContext.ChangeTracker.Clear();

        // ========== ACT ==========
        var result = await _authService.LoginAsync(user.Email, wrongPassword);

        // ========== ASSERT ==========
        result.Should().BeNull();
    }

    [Test]
    [Description("Đăng nhập khi tài khoản không active. Kiểm tra trả về null.")]
    public async Task LoginAsync_InactiveUser_ReturnsNull()
    {
        // ========== ARRANGE ==========
        const string password = "ValidPass123";
        var user = UserFixture.CreateInactiveUser(password: password);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        
        // Clear tracker để tránh conflict khi AuthService.LoginAsync gọi Update
        _dbContext.ChangeTracker.Clear();

        // ========== ACT ==========
        var result = await _authService.LoginAsync(user.Email, password);

        // ========== ASSERT ==========
        result.Should().BeNull();
    }

    [Test]
    [Description("Đăng nhập với email hoặc password rỗng. Kiểm tra trả về null.")]
    public async Task LoginAsync_EmptyCredentials_ReturnsNull()
    {
        // ========== ACT & ASSERT - Empty email ==========
        var result1 = await _authService.LoginAsync("", "password");
        result1.Should().BeNull();

        // ========== ACT & ASSERT - Empty password ==========
        var result2 = await _authService.LoginAsync("user@example.com", "");
        result2.Should().BeNull();

        // ========== ACT & ASSERT - Both empty ==========
        var result3 = await _authService.LoginAsync("", "");
        result3.Should().BeNull();
    }

    #endregion

    #region GenerateJwtToken Tests

    [Test]
    [Description("Sinh JWT Token với dữ liệu hợp lệ. Kiểm tra token chứa Username claim.")]
    public void GenerateJwtToken_ValidInputs_ContainsUsernameClaim()
    {
        // ========== ARRANGE ==========
        const int userId = 1;
        const string username = "testuser";
        const string email = "test@example.com";
        const string role = "Customer";

        // ========== ACT ==========
        var token = _authService.GenerateJwtToken(userId, username, email, role);

        // ========== ASSERT ==========
        token.Should().NotBeNullOrEmpty();

        // Decode token để kiểm tra claims
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Verify standard claims
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == username);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == email);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == role);

        // Verify custom claims
        jwtToken.Claims.Should().Contain(c => c.Type == "Username" && c.Value == username);
        jwtToken.Claims.Should().Contain(c => c.Type == "Email" && c.Value == email);
        jwtToken.Claims.Should().Contain(c => c.Type == "UserId" && c.Value == userId.ToString());

        // Verify token expiration
        jwtToken.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromSeconds(5));
    }

    [Test]
    [Description("Sinh JWT Token với Secret dưới 32 ký tự. Kiểm tra ném InvalidOperationException.")]
    public void GenerateJwtToken_InvalidSecret_ThrowsException()
    {
        // ========== ARRANGE ==========
        var invalidConfig = new Mock<IConfiguration>();
        invalidConfig.Setup(c => c["Jwt:Secret"])
            .Returns("short-secret");  // Less than 32 characters
        invalidConfig.Setup(c => c["Jwt:Issuer"])
            .Returns("CinemaBookingAPI");
        invalidConfig.Setup(c => c["Jwt:Audience"])
            .Returns("CinemaBookingClients");
        invalidConfig.Setup(c => c["Jwt:ExpiryMinutes"])
            .Returns("60");

        var authService = new AuthService(_dbContext, invalidConfig.Object);

        // ========== ACT & ASSERT ==========
        var ex = Assert.Throws<InvalidOperationException>(
            () => authService.GenerateJwtToken(1, "user", "user@example.com", "Customer"));

        ex.Message.Should().Contain("JWT Secret");
        ex.Message.Should().Contain("32 ký tự");
    }

    #endregion

    #region EmailExistsAsync Tests

    [Test]
    [Description("Kiểm tra email đã tồn tại. Trả về true.")]
    public async Task EmailExistsAsync_EmailExists_ReturnsTrue()
    {
        // ========== ARRANGE ==========
        const string email = "existing@example.com";
        var user = UserFixture.CreateValidUser(email: email);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // ========== ACT ==========
        var result = await _authService.EmailExistsAsync(email);

        // ========== ASSERT ==========
        result.Should().BeTrue();
    }

    [Test]
    [Description("Kiểm tra email chưa tồn tại. Trả về false.")]
    public async Task EmailExistsAsync_EmailNotExists_ReturnsFalse()
    {
        // ========== ARRANGE ==========
        const string email = "notexist@example.com";

        // ========== ACT ==========
        var result = await _authService.EmailExistsAsync(email);

        // ========== ASSERT ==========
        result.Should().BeFalse();
    }

    #endregion
}
