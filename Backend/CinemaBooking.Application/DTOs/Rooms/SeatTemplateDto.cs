namespace CinemaBooking.Application.DTOs.Rooms;

public class SeatTemplateDto
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public string Row { get; set; } = string.Empty;
    public int Number { get; set; }
    public string Type { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public int GridRow { get; set; }
    public int GridColumn { get; set; }
}

public class CreateSeatTemplateDto
{
    public required string Row { get; set; }
    public int Number { get; set; }
    public required string Type { get; set; }
    public int GridRow { get; set; }
    public int GridColumn { get; set; }
}

public class BulkCreateSeatTemplateDto
{
    public List<CreateSeatTemplateDto> Seats { get; set; } = new();

    public int TotalSeats { get; set; }
}
