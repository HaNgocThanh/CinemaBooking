using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaBooking.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingIdToShowtimeSeats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BookingId",
                table: "ShowtimeSeats",
                type: "NUMBER(10)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShowtimeSeats_BookingId",
                table: "ShowtimeSeats",
                column: "BookingId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShowtimeSeats_Bookings_BookingId",
                table: "ShowtimeSeats",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShowtimeSeats_Bookings_BookingId",
                table: "ShowtimeSeats");

            migrationBuilder.DropIndex(
                name: "IX_ShowtimeSeats_BookingId",
                table: "ShowtimeSeats");

            migrationBuilder.DropColumn(
                name: "BookingId",
                table: "ShowtimeSeats");
        }
    }
}
