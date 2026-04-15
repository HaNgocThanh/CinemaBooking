# BookingService Unit Tests

## Tổng quan
Bộ test này kiểm thử hàm `BookingService.CreateBookingAsync()` theo **pattern AAA (Arrange-Act-Assert)** với đầy đủ các test cases cho thành công và lỗi.

## Tech Stack
- **Framework**: NUnit 4.1
- **Mock Library**: Moq 4.20
- **Assertion**: FluentAssertions 6.12

## Test Cases

### ✅ Success Cases (Thành công)

#### 1. `CreateBookingAsync_ValidRequest_ReturnsSuccessfulBookingResponse`
**Mục đích**: Kiểm thử tạo booking hợp lệ mà không có khuyến mãi
- **Input**: 
  - 3 ghế (seat IDs: 1, 2, 3)
  - Giá cơ bản: 150,000 VND/vé
  - Không áp dụng mã khuyến mãi
- **Expected Output**:
  - Booking code được tạo
  - Status = `PendingPayment`
  - SubTotal = 450,000 VND (150k x 3)
  - DiscountAmount = 0
  - TotalAmount = 450,000 VND
  - 3 tickets được tạo
  - Transaction committed thành công
- **Kiểm chứng**:
  - Repository methods được gọi đúng số lần
  - DbContext SaveChangesAsync được gọi

#### 2. `CreateBookingAsync_ValidRequestWithValidPromoCode_AppliesDiscountCorrectly`
**Mục đích**: Kiểm thử booking với mã khuyến mãi còn hiệu lực
- **Input**:
  - 2 ghế
  - Giá cơ bản: 100,000 VND/vé
  - Promo code: "SUMMER2024" (20% discount, còn hạn)
- **Expected Output**:
  - SubTotal = 200,000 VND
  - DiscountAmount = 40,000 VND (20% of 200k)
  - TotalAmount = 160,000 VND
  - AppliedPromoCode = "SUMMER2024"
  - Status = `PendingPayment`
- **Kiểm chứng**:
  - Promo validation được gọi
  - Discount được tính chính xác
  - Promotion info được lưu vào Booking entity

---

### ❌ Failure Cases

#### 3. `CreateBookingAsync_SeatAlreadyLocked_ThrowsSeatAlreadyLockedException`
**Mục đích**: Kiểm thử concurrency - ghế bị khóa bởi user khác
- **Scenario**: 
  - ISeatRepository.LockSeatsAsync() ném ra `SeatAlreadyLockedException`
  - Điều này xảy ra khi 2 users cố khóa cùng ghế cùng lúc
- **Expected Behavior**:
  - Exception được rethrow
  - **Transaction rollback tự động** (Không lưu booking)
  - SaveChangesAsync **KHÔNG được gọi**
- **Verify**:
  - Seat lock attempt được ghi lại
  - Database không thay đổi (ROLLBACK)
  - Exception message chứa tên ghế

#### 4. `CreateBookingAsync_PromoCodeExpired_ThrowsPromoExpiredException`
**Mục đích**: Kiểm thử khuyến mãi hết hạn
- **Scenario**:
  - User nhập mã promo "OLD_PROMO"
  - Promo đã hết hạn (EndDate < UtcNow)
  - PromotionInfo.IsValid() return false
- **Expected Behavior**:
  - `PromoExpiredException` được ném
  - Error code = "PROMO_EXPIRED"
  - Transaction rollback
  - SaveChangesAsync **KHÔNG được gọi**
- **Verify**:
  - Promotion retrieval được gọi
  - Ghế vẫn bị khóa (từ bước seize) nhưng booking không được lưu

#### 5. `CreateBookingAsync_EmptySeatIdList_ThrowsInvalidSeatsException`
**Mục đích**: Kiểm thử validation - danh sách ghế trống
- **Scenario**: 
  - BookingRequestDto.SeatIds = [] (rỗng)
- **Expected Behavior**:
  - `InvalidSeatsException` được ném ở bước validation (EARLY)
  - Error code = "INVALID_SEATS"
  - Không gọi repository hoặc database
- **Verify**:
  - Early validation (fail-fast)
  - Không có side effects

#### 6. `CreateBookingAsync_NullRequest_ThrowsArgumentNullException`
**Mục đích**: Kiểm thử null input
- **Scenario**: Request = null
- **Expected Behavior**:
  - `ArgumentNullException` được ném
  - ParamName = "request"
  - Không gọi repository
- **Verify**:
  - Null safety check

#### 7. `CreateBookingAsync_InvalidShowtimeId_ThrowsInvalidShowtimeException`
**Mục đội**: Kiểm thử validation - ShowtimeId không hợp lệ
- **Scenario**: ShowtimeId = -1 (phải > 0)
- **Expected Behavior**:
  - `InvalidShowtimeException` được ném ở bước validation
  - Error message chứa "phải > 0"
- **Verify**:
  - Validation check trước database query

#### 8. `CreateBookingAsync_UnexpectedDatabaseException_RollsBackTransactionAndThrows`
**Mục đích**: Kiểm thử transaction rollback khi có lỗi database
- **Scenario**:
  - SaveChangesAsync() ném `InvalidOperationException` ("Database connection error")
- **Expected Behavior**:
  - Exception được rethrow
  - **Transaction.RollbackAsync() được gọi**
  - Tất cả changes được revert
- **Verify**:
  - Rollback callback được invoke
  - Exception message được giữ lại

---

## Pattern & Naming Convention

### Pattern AAA (Arrange-Act-Assert)
```csharp
[Test]
public async Task CreateBookingAsync_ValidRequest_ReturnsSuccessfulBookingResponse()
{
    // ARRANGE: Setup data, mocks, expectations
    var request = new BookingRequestDto { ... };
    _mockShowtimeRepository.Setup(...);
    
    // ACT: Gọi method cần test
    var result = await _bookingService.CreateBookingAsync(request);
    
    // ASSERT: Verify kết quả
    result.Should().NotBeNull();
    result.Status.Should().Be(BookingStatus.PendingPayment.ToString());
}
```

### Naming Convention: `FunctionName_StateUnderTest_ExpectedBehavior`
- `CreateBookingAsync` = Function name
- `ValidRequest` = State under test (input/condition)
- `ReturnsSuccessfulBookingResponse` = Expected behavior/assertion

---

## Setup & Dependencies

### NUnit Attributes
- `[TestFixture]`: Đánh dấu class chứa tests
- `[SetUp]`: Chạy trước mỗi test (initialize mocks, service)
- `[Test]`: Đánh dấu test method

### Moq Usage
```csharp
// Setup mock behavior
_mockShowtimeRepository.Setup(x => x.GetByIdAsync(id))
    .ReturnsAsync(showtime);

// Verify method was called
_mockShowtimeRepository.Verify(x => x.GetByIdAsync(id), Times.Once);
```

### FluentAssertions
```csharp
// Readable assertions
result.Should().NotBeNull();
result.Status.Should().Be(BookingStatus.PendingPayment.ToString());
result.DiscountAmount.Should().Be(expectedDiscount);
exception.Code.Should().Be("PROMO_EXPIRED");
```

---

## Running Tests

### Via Visual Studio
```bash
Test > Run All Tests
# hoặc
Ctrl + R, A
```

### Via Terminal (dotnet CLI)
```bash
cd src/Backend
dotnet test tests/CinemaBooking.Application.Tests/

# Run specific test class
dotnet test tests/CinemaBooking.Application.Tests/ --filter BookingServiceTests

# Run specific test method
dotnet test tests/CinemaBooking.Application.Tests/ --filter CreateBookingAsync_ValidRequest_ReturnsSuccessfulBookingResponse

# With verbose output
dotnet test tests/CinemaBooking.Application.Tests/ -v detailed
```

### Via NUnit Console (nunit3-console)
```bash
nunit3-console tests/CinemaBooking.Application.Tests/bin/Debug/net9.0/CinemaBooking.Application.Tests.dll
```

---

## Key Testing Points ✅

1. **Pessimistic Locking (Concurrency)**
   - Test thực hiện locking quá trình booking
   - Verify rollback khi có lock conflict

2. **Promotion Validation**
   - Test promo code validation
   - Test discount calculation (PERCENTAGE & FIXED)
   - Test promo expiry check

3. **Input Validation (Fail-Fast)**
   - Null checks
   - Empty lists
   - Invalid IDs
   - All validation errors throw before database access

4. **Transaction Management**
   - Test BeginTransactionAsync được gọi
   - Test CommitAsync sau success
   - Test RollbackAsync sau error

5. **Database Interaction**
   - Test SaveChangesAsync được gọi đúng lần
   - Test Bookings, Tickets, ShowtimeSeats được cập nhật

6. **Mock Isolation**
   - Tất cả dependencies được mock (ISeatRepository, IShowtimeRepository, etc.)
   - ApplicationDbContext được mock (không có real database)
   - Test độc lập, không phụ thuộc vào data thực

---

## Notes

- **Mocking DbContext**: Vì EF Core DbContext phức tạp, test sử dụng Moq.Mock<ApplicationDbContext>
  - Nếu cần test DbContext logic thực, sử dụng `InMemoryDatabase` từ Microsoft.EntityFrameworkCore.InMemory
- **PromotionInfo.IsValid()**: Là method domain logic, test verify nó được gọi đúng
- **Transaction Handling**: Test verify transaction commit/rollback được gọi, nhưng không test logic rollback thực

---

## Future Improvements

- [ ] Add integration tests với InMemoryDatabase (full DbContext logic)
- [ ] Add tests cho ComboRepository (khi implement)
- [ ] Add performance tests (concurrent requests)
- [ ] Add tests cho CodeGenerator (BookingCode, TicketCode generation)
- [ ] Add tests cho edge cases (decimal precision, max booking size)
