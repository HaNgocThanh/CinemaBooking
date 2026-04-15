using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Enums;

namespace CinemaBooking.Domain.Interfaces;

/// <summary>
/// Repository interface cho thực thể ShowtimeSeat.
/// Cung cấp các phương thức để quản lý ghế trong suất chiếu,
/// đặc biệt là pessimistic locking để xử lý concurrent seat booking.
/// </summary>
public interface ISeatRepository
{
    /// <summary>
    /// Khóa một nhóm ghế với pessimistic locking sử dụng Oracle SELECT ... FOR UPDATE NOWAIT.
    /// Nếu ghế đã bị khóa bởi transaction khác, sẽ ném SeatAlreadyLockedException.
    /// </summary>
    /// <param name="seatIds">Danh sách ID ghế cần khóa.</param>
    /// <param name="showtimeId">ID của suất chiếu chứa các ghế.</param>
    /// <param name="userId">ID của người dùng (session ID) khóa ghế.</param>
    /// <returns>Danh sách ghế đã bị khóa với trạng thái Locked.</returns>
    /// <exception cref="SeatAlreadyLockedException">Nếu ghế đã bị khóa bởi user khác (ORA-00054).</exception>
    Task<List<ShowtimeSeat>> LockSeatsAsync(List<int> seatIds, int showtimeId, string userId);

    /// <summary>
    /// Lấy danh sách ghế có trạng thái cụ thể trong một suất chiếu.
    /// </summary>
    /// <param name="showtimeId">ID của suất chiếu.</param>
    /// <param name="status">Trạng thái ghế muốn lấy.</param>
    /// <returns>Danh sách ghế thỏa mãn điều kiện.</returns>
    Task<List<ShowtimeSeat>> GetSeatsByStatusAsync(int showtimeId, SeatStatus status);

    /// <summary>
    /// Lấy ghế theo ID.
    /// </summary>
    Task<ShowtimeSeat?> GetByIdAsync(int seatId);

    /// <summary>
    /// Lấy tất cả ghế của một suất chiếu.
    /// </summary>
    Task<List<ShowtimeSeat>> GetByShowtimeIdAsync(int showtimeId);

    /// <summary>
    /// Cập nhật trạng thái ghế.
    /// </summary>
    Task UpdateAsync(ShowtimeSeat seat);

    /// <summary>
    /// Cập nhật danh sách ghế.
    /// </summary>
    Task UpdateRangeAsync(List<ShowtimeSeat> seats);

    /// <summary>
    /// Lưu thay đổi vào database.
    /// </summary>
    Task SaveChangesAsync();
}
