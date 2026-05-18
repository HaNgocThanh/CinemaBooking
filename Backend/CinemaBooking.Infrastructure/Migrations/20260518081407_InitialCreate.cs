using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CinemaBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Movies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Title = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    Director = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    Cast = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    Genre = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    DurationMinutes = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Language = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    RatingCode = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: true),
                    PosterUrl = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    TrailerUrl = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    BannerUrl = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    IsFeatured = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "DATE", nullable: true),
                    EndDate = table.Column<DateTime>(type: "DATE", nullable: true),
                    Status = table.Column<int>(type: "NUMBER(10)", nullable: false, defaultValue: 0),
                    IsActive = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "DATE", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "DATE", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Name = table.Column<string>(type: "VARCHAR2(50)", nullable: false),
                    Capacity = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Type = table.Column<string>(type: "VARCHAR2(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Username = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    FullName = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: true),
                    Role = table.Column<int>(type: "NUMBER(10)", nullable: false, defaultValue: 0),
                    IsEmailConfirmed = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    IsActive = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "DATE", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "DATE", nullable: true),
                    LastLogin = table.Column<DateTime>(type: "DATE", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SeatTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    RoomId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Row = table.Column<string>(type: "NVARCHAR2(5)", maxLength: 5, nullable: false),
                    Number = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Type = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    GridRow = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    GridColumn = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeatTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeatTemplates_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Showtimes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    MovieId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    RoomId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "DATE", nullable: false),
                    EndTime = table.Column<DateTime>(type: "DATE", nullable: false),
                    BasePrice = table.Column<decimal>(type: "DECIMAL(10,2)", precision: 10, scale: 2, nullable: false),
                    TotalSeats = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    BookedSeatsCount = table.Column<int>(type: "NUMBER(10)", nullable: false, defaultValue: 0),
                    IsActive = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "DATE", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "DATE", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Showtimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Showtimes_Movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Showtimes_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    BookingCode = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: false),
                    ShowtimeId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CustomerId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Status = table.Column<int>(type: "NUMBER(10)", nullable: false, defaultValue: 0),
                    TotalTickets = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    SubTotal = table.Column<decimal>(type: "DECIMAL(15,2)", precision: 15, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "DECIMAL(15,2)", precision: 15, scale: 2, nullable: false, defaultValue: 0m),
                    TotalAmount = table.Column<decimal>(type: "DECIMAL(15,2)", precision: 15, scale: 2, nullable: false),
                    PromotionCode = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    SessionId = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    BookedAt = table.Column<DateTime>(type: "DATE", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "DATE", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "DATE", nullable: true),
                    PaymentMethod = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: true),
                    TransactionId = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "DATE", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_Showtimes_ShowtimeId",
                        column: x => x.ShowtimeId,
                        principalTable: "Showtimes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShowtimeSeats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ShowtimeId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    SeatNumber = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: false),
                    RowLetter = table.Column<string>(type: "NVARCHAR2(5)", maxLength: 5, nullable: false),
                    ColumnNumber = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Status = table.Column<int>(type: "NUMBER(10)", nullable: false, defaultValue: 0),
                    Type = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    LockedAt = table.Column<DateTime>(type: "DATE", nullable: true),
                    HoldExpiryTime = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    LockedBySessionId = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    BookedByUserId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    TicketId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "DATE", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "DATE", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShowtimeSeats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShowtimeSeats_Showtimes_ShowtimeId",
                        column: x => x.ShowtimeId,
                        principalTable: "Showtimes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    TicketCode = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: false),
                    BookingId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ShowtimeSeatId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Price = table.Column<decimal>(type: "DECIMAL(10,2)", precision: 10, scale: 2, nullable: false),
                    SeatType = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: true, defaultValue: "STANDARD"),
                    IsActive = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "DATE", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "DATE", nullable: true),
                    PrintCount = table.Column<int>(type: "NUMBER(10)", nullable: false, defaultValue: 0),
                    LastPrintedAt = table.Column<DateTime>(type: "DATE", nullable: true),
                    Notes = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "DATE", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "DATE", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tickets_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tickets_ShowtimeSeats_ShowtimeSeatId",
                        column: x => x.ShowtimeSeatId,
                        principalTable: "ShowtimeSeats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "Id", "Capacity", "Name", "Type" },
                values: new object[,]
                {
                    { 1, 20, "Phòng 01", "2D" },
                    { 2, 20, "Phòng 02", "3D" }
                });

            migrationBuilder.InsertData(
                table: "SeatTemplates",
                columns: new[] { "Id", "DisplayOrder", "GridColumn", "GridRow", "Number", "RoomId", "Row", "Type" },
                values: new object[,]
                {
                    { 1, 1, 0, 0, 1, 1, "A", 0 },
                    { 2, 2, 0, 0, 2, 1, "A", 0 },
                    { 3, 3, 0, 0, 3, 1, "A", 0 },
                    { 4, 4, 0, 0, 4, 1, "A", 0 },
                    { 5, 5, 0, 0, 5, 1, "A", 0 },
                    { 6, 6, 0, 0, 1, 1, "B", 0 },
                    { 7, 7, 0, 0, 2, 1, "B", 0 },
                    { 8, 8, 0, 0, 3, 1, "B", 0 },
                    { 9, 9, 0, 0, 4, 1, "B", 0 },
                    { 10, 10, 0, 0, 5, 1, "B", 0 },
                    { 11, 11, 0, 0, 1, 1, "C", 1 },
                    { 12, 12, 0, 0, 2, 1, "C", 1 },
                    { 13, 13, 0, 0, 3, 1, "C", 1 },
                    { 14, 14, 0, 0, 4, 1, "C", 1 },
                    { 15, 15, 0, 0, 5, 1, "C", 1 },
                    { 16, 16, 0, 0, 1, 1, "D", 1 },
                    { 17, 17, 0, 0, 2, 1, "D", 1 },
                    { 18, 18, 0, 0, 3, 1, "D", 1 },
                    { 19, 19, 0, 0, 4, 1, "D", 1 },
                    { 20, 20, 0, 0, 5, 1, "D", 1 },
                    { 21, 1, 0, 0, 1, 2, "A", 0 },
                    { 22, 2, 0, 0, 2, 2, "A", 0 },
                    { 23, 3, 0, 0, 3, 2, "A", 0 },
                    { 24, 4, 0, 0, 4, 2, "A", 0 },
                    { 25, 5, 0, 0, 5, 2, "A", 0 },
                    { 26, 6, 0, 0, 1, 2, "B", 0 },
                    { 27, 7, 0, 0, 2, 2, "B", 0 },
                    { 28, 8, 0, 0, 3, 2, "B", 0 },
                    { 29, 9, 0, 0, 4, 2, "B", 0 },
                    { 30, 10, 0, 0, 5, 2, "B", 0 },
                    { 31, 11, 0, 0, 1, 2, "C", 1 },
                    { 32, 12, 0, 0, 2, 2, "C", 1 },
                    { 33, 13, 0, 0, 3, 2, "C", 1 },
                    { 34, 14, 0, 0, 4, 2, "C", 1 },
                    { 35, 15, 0, 0, 5, 2, "C", 1 },
                    { 36, 16, 0, 0, 1, 2, "D", 1 },
                    { 37, 17, 0, 0, 2, 2, "D", 1 },
                    { 38, 18, 0, 0, 3, 2, "D", 1 },
                    { 39, 19, 0, 0, 4, 2, "D", 1 },
                    { 40, 20, 0, 0, 5, 2, "D", 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Booking_ExpiresAt",
                table: "Bookings",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_ShowtimeId",
                table: "Bookings",
                column: "ShowtimeId");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_ShowtimeId_Status",
                table: "Bookings",
                columns: new[] { "ShowtimeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Booking_Status",
                table: "Bookings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CustomerId",
                table: "Bookings",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "UX_Booking_BookingCode",
                table: "Bookings",
                column: "BookingCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SeatTemplate_RoomId",
                table: "SeatTemplates",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "UX_SeatTemplate_RoomId_Row_Number",
                table: "SeatTemplates",
                columns: new[] { "RoomId", "Row", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Showtime_MovieId",
                table: "Showtimes",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_Showtime_StartTime_IsActive",
                table: "Showtimes",
                columns: new[] { "StartTime", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Showtimes_RoomId",
                table: "Showtimes",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_ShowtimeSeat_LockedAt",
                table: "ShowtimeSeats",
                column: "LockedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ShowtimeSeat_LockedBySessionId",
                table: "ShowtimeSeats",
                column: "LockedBySessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ShowtimeSeat_ShowtimeId_Status",
                table: "ShowtimeSeats",
                columns: new[] { "ShowtimeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "UX_ShowtimeSeat_ShowtimeId_SeatNumber",
                table: "ShowtimeSeats",
                columns: new[] { "ShowtimeId", "SeatNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_BookingId",
                table: "Tickets",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_ShowtimeSeatId",
                table: "Tickets",
                column: "ShowtimeSeatId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_UsedAt",
                table: "Tickets",
                column: "UsedAt");

            migrationBuilder.CreateIndex(
                name: "UX_Ticket_TicketCode",
                table: "Tickets",
                column: "TicketCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_CreatedAt",
                table: "Users",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_User_IsActive",
                table: "Users",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "UX_User_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_User_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeatTemplates");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "ShowtimeSeats");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Showtimes");

            migrationBuilder.DropTable(
                name: "Movies");

            migrationBuilder.DropTable(
                name: "Rooms");
        }
    }
}
