using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

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
                    ReleaseDate = table.Column<DateTime>(type: "DATE", nullable: true),
                    EndDate = table.Column<DateTime>(type: "DATE", nullable: true),
                    IsActive = table.Column<bool>(type: "BOOLEAN", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "DATE", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "DATE", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Showtimes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    MovieId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    RoomNumber = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: false),
                    StartTime = table.Column<DateTime>(type: "DATE", nullable: false),
                    EndTime = table.Column<DateTime>(type: "DATE", nullable: false),
                    BasePrice = table.Column<decimal>(type: "DECIMAL(10,2)", precision: 10, scale: 2, nullable: false),
                    TotalSeats = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    BookedSeatsCount = table.Column<int>(type: "NUMBER(10)", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "BOOLEAN", nullable: false, defaultValue: true),
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
                    LockedAt = table.Column<DateTime>(type: "DATE", nullable: true),
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
                    IsActive = table.Column<bool>(type: "BOOLEAN", nullable: false, defaultValue: true),
                    IssuedAt = table.Column<DateTime>(type: "DATE", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "DATE", nullable: true),
                    PrintCount = table.Column<int>(type: "NUMBER(10)", nullable: false, defaultValue: 0),
                    LastPrintedAt = table.Column<DateTime>(type: "DATE", nullable: true),
                    Notes = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "DATE", nullable: false, defaultValue: new DateTime(2026, 4, 15, 16, 6, 42, 623, DateTimeKind.Utc).AddTicks(3801)),
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
                name: "UX_Booking_BookingCode",
                table: "Bookings",
                column: "BookingCode",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "ShowtimeSeats");

            migrationBuilder.DropTable(
                name: "Showtimes");

            migrationBuilder.DropTable(
                name: "Movies");
        }
    }
}
