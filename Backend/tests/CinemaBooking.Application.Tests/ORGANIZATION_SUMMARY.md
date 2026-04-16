# ✅ Test Project Organization Complete

## 📁 Final Structure

```
CinemaBooking.Application.Tests/
├── Services/
│   ├── AuthServiceTests.cs          ← Unit tests cho Authentication
│   └── BookingServiceTests.cs       ← Unit tests cho Booking
│
├── Fixtures/
│   └── EntityFixtures.cs            ← Factory: User, Showtime, Booking test data
│
├── Helpers/
│   └── TestHelpers.cs               ← Builders & Factories:
│                                       • TestDbContextBuilder
│                                       • TestConfigurationBuilder
│                                       • TestDataFactory
│
├── Mocks/
│   └── (Tạo custom mocks khi cần)
│
├── TEST_STRUCTURE.md                ← Chi tiết về cách tổ chức tests
├── MOCK_STRATEGY.md                 ← Mocking patterns & conventions
├── QUICK_START.md                   ← Quick reference
└── README.md                         ← Overview
```

## 🎯 Lợi ích của cấu trúc mới

✅ **Rõ ràng & Dễ navigate** - Tests được sắp xếp theo loại  
✅ **Tái sử dụng** - Fixtures & Helpers dùng lại qua nhiều tests  
✅ **Dễ bảo trì** - Thay đổi test data ở một chỗ  
✅ **Scalable** - Sẵn sàng cho nhiều test services khác  

## 📚 Files Được Tạo

### 1️⃣ AuthServiceTests.cs (Services/)
- ✅ 6 tests cho Register (bao gồm Username validation)
- ✅ 5 tests cho Login (kiểm tra AuthResponse với Username)
- ✅ 2 tests cho GenerateJwtToken (decode & verify claims)
- ✅ 2 tests cho EmailExistsAsync
- **Pattern:** AAA (Arrange-Act-Assert)
- **Mocking:** Moq + DbContext + IConfiguration

### 2️⃣ BookingServiceTests.cs (Services/)
- ✅ 2 success cases (basic & with promo)
- ✅ 3 failure cases (concurrency, promo, validation)
- ✅ 1 transaction rollback case
- **Pattern:** AAA + Region-based organization
- **Testing:** Real in-memory DbContext + Mock repositories

### 3️⃣ EntityFixtures.cs (Fixtures/)
**UserFixture** - Tạo User test data
```csharp
var user = UserFixture.CreateValidUser();
var admin = UserFixture.CreateAdminUser();
var inactive = UserFixture.CreateInactiveUser();
```

**ShowtimeFixture** - Tạo Showtime test data
```csharp
var showtime = ShowtimeFixture.CreateValidShowtime();
var upcoming = ShowtimeFixture.CreateUpcomingShowtime();
var past = ShowtimeFixture.CreatePastShowtime();
```

**BookingFixture** - Tạo Booking test data
```csharp
var booking = BookingFixture.CreateValidBooking();
var withDiscount = BookingFixture.CreateBookingWithDiscount();
var paid = BookingFixture.CreatePaidBooking();
var expired = BookingFixture.CreateExpiredBooking();
```

### 4️⃣ TestHelpers.cs (Helpers/)
**TestDbContextBuilder** - Tạo in-memory DbContext
```csharp
var dbContext = TestDbContextBuilder.CreateInMemoryDbContext();
```

**TestConfigurationBuilder** - Tạo mock IConfiguration
```csharp
var config = TestConfigurationBuilder.CreateMockConfiguration();
var customConfig = TestConfigurationBuilder.CreateMockConfigurationWithCustomValue(
    "key", "value");
```

**TestDataFactory** - Generate unique IDs & codes
```csharp
var userId = TestDataFactory.GenerateUserId();
var bookingCode = TestDataFactory.GenerateBookingCode();
```

## 🚀 Cách Sử Dụng

### Ví dụ 1: Sử dụng Fixture trong test
```csharp
[SetUp]
public void Setup()
{
    var user = UserFixture.CreateValidUser(
        username: "myuser",
        email: "my@example.com");
    _dbContext.Users.Add(user);
    _dbContext.SaveChanges();
}
```

### Ví dụ 2: Sử dụng TestDbContextBuilder
```csharp
[SetUp]
public void Setup()
{
    _dbContext = TestDbContextBuilder.CreateInMemoryDbContext();
    _authService = new AuthService(_dbContext, _mockConfig);
}
```

### Ví dụ 3: Sử dụng TestConfigurationBuilder
```csharp
[SetUp]
public void Setup()
{
    _mockConfig = TestConfigurationBuilder.CreateMockConfiguration(
        jwtSecret: "custom-secret-key-32-chars-long",
        jwtExpiryMinutes: "120");
    _authService = new AuthService(_dbContext, _mockConfig);
}
```

## 📊 Test Coverage

| Component | Tests | Status |
|-----------|-------|--------|
| AuthService | 15 | ✅ |
| BookingService | 8 | ✅ |
| Fixtures | - | ✅ |
| Helpers | - | ✅ |

## 🔧 Chạy Tests

```bash
# Chạy tất cả tests
dotnet test

# Chạy AuthServiceTests
dotnet test --filter "FullyQualifiedName~AuthServiceTests"

# Chạy BookingServiceTests
dotnet test --filter "FullyQualifiedName~BookingServiceTests"

# Chạy với verbose output
dotnet test --verbosity detailed

# Chạy và tạo coverage report
dotnet test /p:CollectCoverage=true
```

## ✨ Tiếp Theo

1. **Thêm Services Tests khác:**
   - MovieServiceTests
   - PromotionServiceTests
   - PaymentServiceTests

2. **Thêm Fixtures cho các entities:**
   - MovieFixture
   - PromotionFixture
   - TicketFixture

3. **Tạo Integration Tests** (nếu cần):
   - Folder: `Integration/`
   - Kiểm tra API endpoints với real database

4. **Tạo Performance Tests** (optional):
   - Folder: `Performance/`
   - Kiểm tra concurrency, pessimistic locking

---

**Last Updated:** April 17, 2026  
**Test Framework:** NUnit 4.1.0  
**Mocking Library:** Moq 4.20.70  
**Assertion Library:** FluentAssertions 6.12.0
