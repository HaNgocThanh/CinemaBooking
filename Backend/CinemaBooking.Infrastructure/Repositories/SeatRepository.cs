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
    /// Quy trình:
    /// 1. Bắt đầu transaction
    /// 2. Raw SQL SELECT FOR UPDATE NOWAIT trên các ghế
    /// 3. Kiểm tra trạng thái ghế (phải là Available)
    /// 4. Cập nhật trạng thái thành Locked
    /// 5. Lưu changes
    /// 
    /// Nếu ghế đã bị khóa bởi transaction khác, Oracle sẽ ném ORA-00054,
    /// repository sẽ bắt và ném SeatAlreadyLockedException.
    /// </summary>
    /// <param name="seatIds">Danh sách ID ghế cần khóa.</param>
    /// <param name="showtimeId">ID của suất chiếu.</param>
    /// <param name="userId">Session ID hoặc User ID khóa ghế.</param>
    /// <returns>Danh sách ShowtimeSeat đã bị khóa.</returns>
    /// <exception cref="SeatAlreadyLockedException">Khi ghế đã bị khóa bởi user khác.</exception>
    public async Task<List<ShowtimeSeat>> LockSeatsAsync(List<int> seatIds, int showtimeId, string userId)
    {
        if (seatIds == null || seatIds.Count == 0)
            throw new ArgumentException("Danh sách ghế không thể trống.", nameof(seatIds));

        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID không thể trống.", nameof(userId));

        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // Bước 1: Sử dụng Raw SQL SELECT ... FOR UPDATE NOWAIT
                // Điều này sẽ lock các ghế Available trên Oracle Database
                // Nếu ghế đã bị lock bởi transaction khác, sẽ ném ORA-00054 ngay lập tức
                var seatIdList = string.Join(",", seatIds);
                var statusAvailable = (int)SeatStatus.Available;
                
                var sql = $"""
                    SELECT * FROM ShowtimeSeats 
                    WHERE Id IN ({seatIdList})
                      AND ShowtimeId = {showtimeId}
                      AND Status = {statusAvailable}
                    FOR UPDATE NOWAIT
                """;

                var lockedSeats = await _context.ShowtimeSeats
                    .FromSqlRaw(sql)
                    .ToListAsync();

                // Bước 2: Kiểm tra xem số ghế lock được có trùng với yêu cầu không
                if (lockedSeats.Count != seatIds.Count)
                {
                    await transaction.RollbackAsync();
                    throw new SeatAlreadyLockedException(
                        "SEAT_NOT_AVAILABLE",
                        $"Một hoặc nhiều ghế không khả dụng. Chỉ có {lockedSeats.Count} trên {seatIds.Count} ghế là có sẵn."
                    );
                }

                // Bước 3: Cập nhật trạng thái thành Locked (trong transaction đã lock)
                var utcNow = DateTime.UtcNow;
                foreach (var seat in lockedSeats)
                {
                    seat.Status = SeatStatus.Locked;
                    seat.LockedAt = utcNow;
                    seat.LockedBySessionId = userId;
                    seat.UpdatedAt = utcNow;
                }

                // Bước 4: Lưu changes vào database
                _context.ShowtimeSeats.UpdateRange(lockedSeats);
                await _context.SaveChangesAsync();

                // Bước 5: Commit transaction (lock sẽ được release)
                await transaction.CommitAsync();

                return lockedSeats;
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();

                // Bắt lỗi ORA-00054: Resource busy (ghế đã bị khóa bởi user khác)
                // Điều này xảy ra khi SELECT FOR UPDATE NOWAIT không thể lock vì ghế đang bị lock
                if (dbEx.InnerException?.Message.Contains("ORA-00054") == true ||
                    dbEx.InnerException?.ToString().Contains("ORA-00054") == true)
                {
                    throw new SeatAlreadyLockedException(
                        "SEAT_LOCKED_BY_ANOTHER_USER",
                        "Một hoặc nhiều ghế đang bị khóa bởi người dùng khác. Vui lòng chọn ghế khác.",
                        dbEx
                    );
                }

                // Nếu là lỗi khác, rethrow
                throw;
            }
            catch (SeatAlreadyLockedException)
            {
                // Re-throw custom exception
                throw;
            }
            catch (Exception)
            {
                // Rollback transaction nếu có bất kỳ lỗi nào
                await transaction.RollbackAsync();
                throw;
            }
        }
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
            seat.UpdatedAt = DateTime.UtcNow;
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
}
