# 📋 Test Structure Guide

## Cấu trúc Thư mục Test

```
CinemaBooking.Application.Tests/
├── Services/
│   ├── AuthServiceTests.cs          (UT cho Auth Service)
│   └── BookingServiceTests.cs       (UT cho Booking Service)
├── Fixtures/
│   ├── UserFixture.cs              (Test data cho User entity)
│   └── BookingFixture.cs           (Test data cho Booking entity)
├── Helpers/
│   ├── TestDbContextBuilder.cs     (Helper: tạo DbContext test)
│   └── MockDataFactory.cs          (Helper: factory cho mock data)
├── Mocks/
│   ├── MockRepositories.cs         (Custom mock instances)
│   └── TestConfigurationBuilder.cs (Helper: tạo IConfiguration mock)
├── QUICK_START.md                  (Quick reference)
├── MOCK_STRATEGY.md                (Mocking patterns & conventions)
└── README.md                        (Overview)
```

## 📝 Quy tắc Organize Tests

### 1. **Services/** - Unit Tests cho Services
- Tên file: `{ServiceName}Tests.cs`
- Pattern: Moq + FluentAssertions + NUnit
- AAA Pattern: Arrange → Act → Assert

**Ví dụ:** `AuthServiceTests.cs`, `BookingServiceTests.cs`

```csharp
[TestFixture]
public class AuthServiceTests
{
    [SetUp] public void Setup() { }
    
    #region Register Tests
    [Test]
    public async Task RegisterAsync_ValidData_ReturnsSuccess() { }
    #endregion
}
```

### 2. **Fixtures/** - Test Data & Builders
- Tên file: `{EntityName}Fixture.cs`
- Tác dụng: Cung cấp test data, entity builders
- Sử dụng: Builder Pattern hoặc Factory Pattern

**Ví dụ:** `UserFixture.cs`
```csharp
public class UserFixture
{
    public static User CreateValidUser(
        string username = "testuser",
        string email = "test@example.com")
    {
        return new User
        {
            Username = username,
            Email = email,
            FullName = "Test User",
            PasswordHash = BCrypt.HashPassword("Password123"),
            Role = UserRole.Customer,
            IsActive = true
        };
    }
}
```

### 3. **Helpers/** - Utility Functions
- Tên file: `{Purpose}Builder.cs` hoặc `{Purpose}Helper.cs`
- Tác dụng: Builder DbContext, tạo mock configuration
- Sử dụng: Tái dùng qua nhiều test classes

**Ví dụ:** `TestDbContextBuilder.cs`
```csharp
public class TestDbContextBuilder
{
    public static ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"Test_{Guid.NewGuid()}")
            .Options;
        return new ApplicationDbContext(options);
    }
}
```

### 4. **Mocks/** - Custom Mock Implementations
- Tên file: `Mock{ComponentName}.cs` hoặc `{ComponentName}Mock.cs`
- Tác dụng: Mock repositories, services, configurations
- Sử dụng: Khi Moq không đủ, cần custom behavior

**Ví dụ:** `TestConfigurationBuilder.cs`
```csharp
public class TestConfigurationBuilder
{
    public static IConfiguration CreateMockConfiguration()
    {
        var config = new Mock<IConfiguration>();
        config.Setup(c => c["Jwt:Secret"])
            .Returns("very-long-secret-key-min-32-chars");
        return config.Object;
    }
}
```

## 🎯 Best Practices

### Naming Conventions
- **Test Methods**: `{MethodName}_{Scenario}_{ExpectedResult}`
  - ✅ `RegisterAsync_ValidData_ReturnsSuccess`
  - ✅ `LoginAsync_InvalidPassword_ReturnsNull`
  - ❌ `TestRegister`, `RegisterTest`

- **Test Classes**: `{ServiceName}Tests`
  - ✅ `AuthServiceTests`
  - ❌ `TestAuthService`, `AuthTest`

### Test Organization
```csharp
[TestFixture]
public class AuthServiceTests
{
    #region Register Tests
    [Test] public async Task RegisterAsync_ValidData_ReturnsSuccess() { }
    [Test] public async Task RegisterAsync_UsernameEmpty_ThrowsException() { }
    #endregion

    #region Login Tests
    [Test] public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse() { }
    #endregion

    #region GenerateJwtToken Tests
    [Test] public void GenerateJwtToken_ValidInputs_ContainsUsernameClaim() { }
    #endregion
}
```

### AAA Pattern
```csharp
[Test]
public async Task TestMethod()
{
    // ========== ARRANGE ==========
    var input = new SomeDto { ... };
    var mock = new Mock<IRepository>();
    
    // ========== ACT ==========
    var result = await _service.DoSomethingAsync(input);
    
    // ========== ASSERT ==========
    result.Should().NotBeNull();
    mock.Verify(..., Times.Once);
}
```

### Assertions
```csharp
// ✅ FluentAssertions
result.Should().NotBeNull();
result.Username.Should().Be("testuser");
result.Roles.Should().Contain("Customer");
exception.Message.Should().Contain("not found");

// ✅ Verify Mocks
mock.Verify(x => x.GetByIdAsync(1), Times.Once);
mock.Verify(x => x.SaveAsync(It.IsAny<User>()), Times.AtLeastOnce);
```

## 🚀 Running Tests

```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~AuthServiceTests"

# Run with coverage
dotnet test /p:CollectCoverage=true

# Run tests in watch mode (requires dotnet-watch)
dotnet watch test
```

## 📚 References

- [NUnit Documentation](https://docs.nunit.org/)
- [Moq Quick Start](https://github.com/moq/moq4/wiki/Quickstart)
- [FluentAssertions Guide](https://fluentassertions.com/introduction)
- [AAA Pattern](https://www.arrange-act-assert.com/)
