namespace CinemaBooking.Application.DTOs.Rooms;

public class SeatTemplateDto
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public string Row { get; set; } = string.Empty;
    public int Number { get; set; }
    public string Type { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
