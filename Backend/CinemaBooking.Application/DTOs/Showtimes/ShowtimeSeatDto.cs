namespace CinemaBooking.Application.DTOs.Showtimes;

/// <summary>
/// DTO phản hồi thông tin ghế trong một suất chiếu.
/// </summary>
public class ShowtimeSeatDto
{
    public int Id { get; set; }
    public int ShowtimeId { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public string RowLetter { get; set; } = string.Empty;
    public int ColumnNumber { get; set; }
    public int GridRow { get; set; }
    public int GridColumn { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
