using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Enums;
using CinemaBooking.Domain.Exceptions;
using CinemaBooking.Domain.Interfaces;
using CinemaBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho thực thể ShowtimeSeat.
/// Quản lý ghế trong suất chiếu với pessimistic locking để xử lý concurrent booking.
/// Sử dụng Oracle SELECT ... FOR UPDATE NOWAIT để ngăn race condition.
/// </summary>
public class SeatRepository : ISeatRepository
{
    private readonly ApplicationDbContext _context;

    public SeatRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Khóa một nhóm ghế với pessimistic locking (Oracle SELECT ... FOR UPDATE NOWAIT).
    ///
    /// IMPORTANT: Phương thức này KHÔNG tự tạo transaction.
    /// Nó phụ thuộc vào transaction mà caller đã mở (VD: BookingService.CreateBookingAsync).
    /// Nếu gọi mà không có transaction, sẽ ném InvalidOperationException.
    ///
    /// Quy trình:
    /// 1. Raw SQL SELECT FOR UPDATE NOWAIT trên các ghế (giữ lock Oracle row)
    /// 2. Kiểm tra trạng thái ghế (phải là Available)
    /// 3. Cập nhật trạng thái thành Locked
    /// 4. Lưu changes — KHÔNG commit (caller commit)
    /// </summary>
    /// <param name="seatIds">Danh sách ID ghế cần khóa.</param>
    /// <param name="showtimeId">ID của suất chiếu.</param>
    /// <param name="userId">Session ID hoặc User ID khóa ghế.</param>
    /// <returns>Danh sách ShowtimeSeat đã bị khóa.</returns>
    /// <exception cref="InvalidOperationException">Khi không có transaction hiện hoạt.</exception>
    /// <exception cref="SeatAlreadyLockedException">Khi ghế đã bị khóa bởi user khác.</exception>
    public async Task<List<ShowtimeSeat>> LockSeatsAsync(List<int> seatIds, int showtimeId, string userId)
    {
        if (seatIds == null || seatIds.Count == 0)
            throw new ArgumentException("Danh sách ghế không thể trống.", nameof(seatIds));

        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID không thể trống.", nameof(userId));

        // Phải có transaction đang mở — đây là Oracle row lock, không phải table lock
        if (_context.Database.CurrentTransaction == null)
        {
            throw new InvalidOperationException(
                "LockSeatsAsync requires an active database transaction. " +
                "Call within BeginTransactionAsync() scope.");
        }

        // Sử dụng Raw SQL SELECT ... FOR UPDATE NOWAIT
        // Lock các ghế Available trên Oracle Database
        // Nếu ghế đã bị lock bởi transaction khác, sẽ ném ORA-00054 ngay lập tức
        var seatIdList = string.Join(",", seatIds);
        var statusAvailable = (int)SeatStatus.Available;

        var sql = $"""
            SELECT * FROM "ShowtimeSeats"
            WHERE "Id" IN ({seatIdList})
              AND "ShowtimeId" = {showtimeId}
              AND "Status" = {statusAvailable}
            FOR UPDATE WAIT 3
        """;

        List<ShowtimeSeat> lockedSeats;

        try
        {
            lockedSeats = await _context.ShowtimeSeats
                .FromSqlRaw(sql)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            // Bắt lỗi ORA-00054: Resource busy (ghế đã bị khóa bởi user khác)
            if (ContainsOra00054(ex))
            {
                throw new SeatAlreadyLockedException(
                    "SEAT_LOCKED_BY_ANOTHER_USER",
                    "Một hoặc nhiều ghế đang bị khóa bởi người dùng khác. Vui lòng chọn ghế khác.",
                    ex
                );
            }
            throw;
        }

        // Kiểm tra xem số ghế lock được có trùng với yêu cầu không
        if (lockedSeats.Count != seatIds.Count)
        {
            throw new SeatAlreadyLockedException(
                "SEAT_NOT_AVAILABLE",
                $"Một hoặc nhiều ghế không khả dụng. Chỉ có {lockedSeats.Count} trên {seatIds.Count} ghế là có sẵn."
            );
        }

        // Cập nhật trạng thái thành Locked
        var lockTime = DateTime.UtcNow;
        foreach (var seat in lockedSeats)
        {
            seat.Status = SeatStatus.Locked;
            seat.LockedAt = lockTime;
            seat.LockedBySessionId = userId;
            seat.UpdatedAt = lockTime;
        }

        // Lưu changes — KHÔNG commit (transaction do caller quản lý)
        _context.ShowtimeSeats.UpdateRange(lockedSeats);
        await _context.SaveChangesAsync();

        return lockedSeats;
    }

    /// <summary>
    /// Lấy danh sách ghế theo trạng thái trong suất chiếu.
    /// </summary>
    public async Task<List<ShowtimeSeat>> GetSeatsByStatusAsync(int showtimeId, SeatStatus status)
    {
        return await _context.ShowtimeSeats
            .Where(s => s.ShowtimeId == showtimeId && s.Status == status)
            .OrderBy(s => s.RowLetter)
            .ThenBy(s => s.ColumnNumber)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Lấy ghế theo ID.
    /// </summary>
    public async Task<ShowtimeSeat?> GetByIdAsync(int seatId)
    {
        return await _context.ShowtimeSeats
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == seatId);
    }

    /// <summary>
    /// Lấy tất cả ghế của suất chiếu.
    /// </summary>
    public async Task<List<ShowtimeSeat>> GetByShowtimeIdAsync(int showtimeId)
    {
        return await _context.ShowtimeSeats
            .Where(s => s.ShowtimeId == showtimeId)
            .OrderBy(s => s.RowLetter)
            .ThenBy(s => s.ColumnNumber)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Cập nhật một ghế.
    /// </summary>
    public async Task UpdateAsync(ShowtimeSeat seat)
    {
        if (seat == null)
            throw new ArgumentNullException(nameof(seat));

        seat.UpdatedAt = DateTime.UtcNow;
        _context.ShowtimeSeats.Update(seat);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Cập nhật danh sách ghế.
    /// </summary>
    public async Task UpdateRangeAsync(List<ShowtimeSeat> seats)
    {
        if (seats == null || seats.Count == 0)
            throw new ArgumentException("Danh sách ghế không thể trống.", nameof(seats));

        foreach (var seat in seats)
        {
            seat.UpdatedAt = DateTime.Now;
        }

        _context.ShowtimeSeats.UpdateRange(seats);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Lưu thay đổi vào database.
    /// </summary>
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Kiểm tra exception chain có chứa mã lỗi ORA-00054 hay không.
    /// Oracle.ManagedDataAccess có thể ném OracleException trực tiếp hoặc wrapped
    /// trong DbUpdateException/InvalidOperationException.
    /// </summary>
    private static bool ContainsOra00054(Exception? ex)
    {
        while (ex != null)
        {
            if (ex.Message.Contains("ORA-00054"))
                return true;
            ex = ex.InnerException;
        }
        return false;
    }
}
