# Mock Strategy & Implementation Notes

## Mocking Architecture

### Các Objects được Mock

#### 1. **ApplicationDbContext**
```csharp
var _mockDbContext = new Mock<ApplicationDbContext>(
    new DbContextOptions<ApplicationDbContext>()
);
```

**Tại sao mock?**
- Tránh dependency vào real database (Oracle)
- Test isolation - không ảnh hưởng tới data thực
- Kiểm tra gọi SaveChangesAsync() bao nhiêu lần

**Mock Methods:**
- `Database.BeginTransactionAsync()` → Trả về mock IDbContextTransaction
- `SaveChangesAsync()` → Trả về 1 (success)
- `Bookings.Add()`, `Tickets.AddRange()`, `ShowtimeSeats.UpdateRange()`

---

#### 2. **ISeatRepository**
```csharp
var _mockSeatRepository = new Mock<ISeatRepository>();
```

**Mục đích**: Isolate seat locking logic

**Setup options:**
```csharp
// Success case
_mockSeatRepository.Setup(x => x.LockSeatsAsync(seatIds, showtimeId, userId))
    .ReturnsAsync(lockedSeats);

// Failure case - Concurrency
_mockSeatRepository.Setup(x => x.LockSeatsAsync(...))
    .ThrowsAsync(new SeatAlreadyLockedException(...));
```

**Verify calls:**
```csharp
_mockSeatRepository.Verify(
    x => x.LockSeatsAsync(seatIds, showtimeId, sessionId),
    Times.Once
);
```

---

#### 3. **IShowtimeRepository**
```csharp
var _mockShowtimeRepository = new Mock<IShowtimeRepository>();
```

**Setup:**
```csharp
_mockShowtimeRepository.Setup(x => x.GetByIdAsync(showtimeId))
    .ReturnsAsync(showtime);

// Failure case - Showtime not found
_mockShowtimeRepository.Setup(x => x.GetByIdAsync(999))
    .ReturnsAsync((Showtime?)null);
```

---

#### 4. **IPromotionRepository**
```csharp
var _mockPromotionRepository = new Mock<IPromotionRepository>();
```

**Setup:**
```csharp
// Valid promo
var promotion = new PromotionInfo
{
    Code = "SUMMER2024",
    DiscountType = "PERCENTAGE",
    DiscountValue = 20m, // 20%
    EndDate = DateTime.UtcNow.AddDays(30) // Not expired
};
_mockPromotionRepository.Setup(x => x.GetByCodeAsync("SUMMER2024"))
    .ReturnsAsync(promotion);

// Expired promo
_mockPromotionRepository.Setup(x => x.GetByCodeAsync("OLDCODE"))
    .ReturnsAsync(expiredPromotion); // EndDate < UtcNow
```

---

#### 5. **ITicketRepository**
```csharp
var _mockTicketRepository = new Mock<ITicketRepository>();
```

**Lưu ý**: Trong hiện tại BookingService, repository này không được sử dụng trong CreateBookingAsync
- Nó được inject vào constructor nhưng không được gọi
- Có thể là placeholder cho feature sau

---

## Transaction Mocking

### Pattern: BeginTransactionAsync + CommitAsync/RollbackAsync

```csharp
var mockTransaction = new Mock<IDbContextTransaction>();

_mockDbContext
    .Setup(x => x.Database.BeginTransactionAsync(default))
    .ReturnsAsync(mockTransaction.Object);

// Verify rollback khi error
mockTransaction.Verify(x => x.RollbackAsync(default), Times.Once);

// Verify commit khi success
mockTransaction.Verify(x => x.CommitAsync(default), Times.Once);
```

---

## Entity Creation Pattern

### Showtime Entity
```csharp
var showtime = new Showtime
{
    Id = 1,
    MovieId = 1,
    RoomNumber = "A1",  // ⚠️ NOT ScreenId
    StartTime = DateTime.UtcNow.AddHours(2),
    EndTime = DateTime.UtcNow.AddHours(4),
    BasePrice = 150000m, // 150k VND
    TotalSeats = 100,
    BookedSeatsCount = 0,
    IsActive = true
};
```

### ShowtimeSeat Entity
```csharp
var seat = new ShowtimeSeat
{
    Id = 1,
    ShowtimeId = 1,
    SeatNumber = "A1",     // "A1", "B5", etc.
    RowLetter = "A",       // "A", "B", "C", etc.
    ColumnNumber = 1,      // 1, 2, 3, etc.
    Status = SeatStatus.Locked,
    LockedAt = DateTime.UtcNow,
    LockedBySessionId = "session-user-123"
};
```

### PromotionInfo (DTO, không phải Entity)
```csharp
var promotion = new PromotionInfo
{
    Code = "SUMMER2024",
    DiscountType = "PERCENTAGE",  // "PERCENTAGE" or "FIXED" (string, not enum)
    DiscountValue = 20m,           // 20% or 50000 VND
    MaxDiscountAmount = null,      // Cap for percentage discounts
    RemainingQuantity = 100,       // null = unlimited
    StartDate = DateTime.UtcNow.AddDays(-1),
    EndDate = DateTime.UtcNow.AddDays(30)
};
```

### Booking Entity
```csharp
var booking = new Booking
{
    Id = 1, // Auto-increment
    BookingCode = "BK2601160000000123456789", // Generated
    ShowtimeId = 1,
    CustomerId = null, // Optional
    Status = BookingStatus.PendingPayment,
    TotalTickets = 3,
    SubTotal = 450000m,
    DiscountAmount = 0m,
    TotalAmount = 450000m,
    PromotionCode = null,
    SessionId = "session-user-123",
    BookedAt = DateTime.UtcNow,
    ExpiresAt = DateTime.UtcNow.AddMinutes(5)
};
```

### Ticket Entity
```csharp
var ticket = new Ticket
{
    Id = 1, // Auto-increment
    TicketCode = "TK2601160000000987654321", // Generated
    BookingId = 1,
    ShowtimeSeatId = 1,
    Price = 150000m,
    SeatType = "STANDARD", // "STANDARD", "VIP", "COUPLE", etc.
    IsActive = true,
    IssuedAt = DateTime.UtcNow,
    CreatedAt = DateTime.UtcNow
};
```

---

## Assertion Patterns

### FluentAssertions Syntax

```csharp
// Null checks
result.Should().NotBeNull();
result.BookingCode.Should().BeNullOrEmpty();

// Numeric comparisons
result.SubTotal.Should().Be(150000m);
result.DiscountAmount.Should().BeGreaterThan(0);
result.TotalAmount.Should().BeLessThan(result.SubTotal);

// String checks
result.Status.Should().Be(BookingStatus.PendingPayment.ToString());
result.AppliedPromoCode.Should().Contain("SUMMER");

// Collection checks
result.TicketIds.Should().HaveCount(3);
result.TicketIds.Should().NotBeEmpty();
result.TicketIds.Should().ContainInOrder(new[] { 1, 2, 3 });

// DateTime checks
result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
result.BookedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));

// Exception checks
var exception = Assert.ThrowsAsync<PromoExpiredException>(
    () => _service.CreateBookingAsync(request)
);
exception.Code.Should().Be("PROMO_EXPIRED");
exception.UserMessage.Should().Contain("hết hạn");
```

---

## Verify Pattern (Moq)

```csharp
// Verify exact call count
_mockShowtimeRepository.Verify(
    x => x.GetByIdAsync(1),
    Times.Once
);

// Verify never called
_mockDbContext.Verify(
    x => x.SaveChangesAsync(default),
    Times.Never
);

// Verify any call with matching args
_mockSeatRepository.Verify(
    x => x.LockSeatsAsync(
        It.IsAny<List<int>>(),
        It.IsAny<int>(),
        It.IsAny<string>()
    ),
    Times.Once
);

// Verify call with specific args
_mockPromotionRepository.Verify(
    x => x.GetByCodeAsync("SUMMER2024"),
    Times.Once
);
```

---

## Important Notes for Test Writer

### ⚠️ Key Points

1. **DoNotUseRealDatabase**
   - Tất cả tests phải mock DbContext
   - Không có data từ Oracle
   - Test chạy fast (không I/O)

2. **TransactionRollback**
   - Khi error xảy ra, BookingService gọi transaction.RollbackAsync()
   - Test verify này được gọi
   - **Không test logic rollback thực** (vì mock transaction)

3. **PromotionInfo.IsValid()**
   - Là domain method kiểm tra StartDate, EndDate, RemainingQuantity
   - Test gọi nó bằng cách tạo PromotionInfo với expired dates
   - Service gọi method này để decide throw PromoExpiredException

4. **SeatId vs ShowtimeSeatId**
   - SeatIds trong request là danh sách ID (1, 2, 3)
   - LockSeatsAsync() trả về List<ShowtimeSeat> objects
   - Test phải map correctly

5. **String vs Enum**
   - BookingStatus là enum
   - Response.Status phải convert to string `.ToString()`
   - DiscountType là string ("PERCENTAGE" hoặc "FIXED")

6. **Decimal Precision**
   - VND currency: decimal với 2 decimal places
   - Use `150000m` hoặc `150000.00m`
   - Avoid double hoặc float (precision loss)

7. **DateTime.UtcNow vs DateTime.Now**
   - Tất cả entities sử dụng **UTC** (DateTime.UtcNow)
   - Test phải mock DateTime hoặc accept small time differences

8. **Early Validation (Fail-Fast)**
   - ValidateInput() throws ngay không hit database
   - Test verify không gọi repository khi validation fails
   - Tránh unnecessary database calls

---

## Common Pitfalls to Avoid ❌

1. **❌ Don't use real database in tests**
   ✅ Always mock repositories

2. **❌ Don't forget to setup mocks**
   ✅ Setup all mock returns before Act

3. **❌ Don't verify private methods**
   ✅ Test only public behavior

4. **❌ Don't test too much in one test**
   ✅ One test = one scenario (AAA pattern)

5. **❌ Don't ignore transaction behavior**
   ✅ Verify rollback on error

6. **❌ Don't use hardcoded IDs that might conflict**
   ✅ Use unique IDs per test case (1, 2, 3, etc. for different tests)

---

## Future Test Enhancements

- [ ] Integration tests với InMemoryDatabase (test real DbContext)
- [ ] Performance tests (concurrent lock attempts)
- [ ] Edge case tests (max booking size, decimal overflow)
- [ ] Snapshot tests (verify exact response structure)
- [ ] Parameterized tests (multiple promo discount types)
