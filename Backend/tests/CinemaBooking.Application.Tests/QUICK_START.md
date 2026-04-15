# Quick Start Guide - Run Unit Tests

## Trước tiên: Kiểm tra project dependencies

```bash
cd d:\CinemaBooking\src\Backend\tests\CinemaBooking.Application.Tests
dotnet restore
```

## Cách 1: Chạy tất cả tests BookingService

```bash
cd d:\CinemaBooking\src\Backend
dotnet test tests/CinemaBooking.Application.Tests/BookingServiceTests.cs
```

## Cách 2: Chạy tests riêng từng case

```bash
# Chạy SUCCESS case
dotnet test --filter "CreateBookingAsync_ValidRequest_ReturnsSuccessfulBookingResponse"

# Chạy PROMO case
dotnet test --filter "CreateBookingAsync_ValidRequestWithValidPromoCode_AppliesDiscountCorrectly"

# Chạy CONCURRENCY case
dotnet test --filter "CreateBookingAsync_SeatAlreadyLocked_ThrowsSeatAlreadyLockedException"

# Chạy PROMO EXPIRED case
dotnet test --filter "CreateBookingAsync_PromoCodeExpired_ThrowsPromoExpiredException"

# Chạy VALIDATION cases
dotnet test --filter "EmptySeatIdList"
dotnet test --filter "NullRequest"
dotnet test --filter "InvalidShowtimeId"
```

## Cách 3: Chạy tất cả tests trong project

```bash
cd d:\CinemaBooking\src\Backend
dotnet test tests/CinemaBooking.Application.Tests/
```

## Verbose Output (Xem chi tiết)

```bash
dotnet test tests/CinemaBooking.Application.Tests/ -v detailed
```

## Expected Results ✅

Nếu tất cả tests PASS, bạn sẽ thấy:

```
Test Run Successful.
Total tests: 8
Passed: 8
Failed: 0
```

---

## Test Cases Summary

| # | Test Name | Category | Expected | 
|---|-----------|----------|----------|
| 1 | ValidRequest_ReturnsSuccessfulBookingResponse | ✅ Success | PASS |
| 2 | ValidRequestWithValidPromoCode_AppliesDiscountCorrectly | ✅ Success | PASS |
| 3 | SeatAlreadyLocked_ThrowsSeatAlreadyLockedException | ❌ Concurrency | PASS (throws exception) |
| 4 | PromoCodeExpired_ThrowsPromoExpiredException | ❌ Promo | PASS (throws exception) |
| 5 | EmptySeatIdList_ThrowsInvalidSeatsException | ❌ Validation | PASS (throws exception) |
| 6 | NullRequest_ThrowsArgumentNullException | ❌ Validation | PASS (throws exception) |
| 7 | InvalidShowtimeId_ThrowsInvalidShowtimeException | ❌ Validation | PASS (throws exception) |
| 8 | UnexpectedDatabaseException_RollsBackTransactionAndThrows | ❌ Transaction | PASS (throws exception) |

---

## Common Issues & Troubleshooting

### ❌ Error: "NUnit not found"
```bash
# Solution: Restore packages
cd tests/CinemaBooking.Application.Tests
dotnet restore
```

### ❌ Error: "Project references not resolved"
```bash
# Solution: Build solution trước
cd d:\CinemaBooking\src\Backend
dotnet build
```

### ❌ Error: "Cannot find type PromotionInfo"
Đảm bảo CinemaBooking.Domain.Interfaces project được reference đúng trong .csproj

### ❌ One or more tests failed
- Check console output để xem error message
- Verify mock setup đúng
- Verify database/entity schema phù hợp

---

## Next Steps

1. ✅ Kiểm tra test chạy & pass
2. ⬜ Implement integration tests (InMemoryDatabase)
3. ⬜ Add tests cho edge cases
4. ⬜ Setup CI/CD pipeline (.github/workflows hoặc Azure DevOps)

---

## References

- [NUnit Documentation](https://docs.nunit.org/)
- [Moq GitHub](https://github.com/moq/moq4)
- [FluentAssertions Docs](https://fluentassertions.com/)
