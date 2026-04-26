using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoomNumber",
                table: "Showtimes");

            migrationBuilder.AddColumn<int>(
                name: "RoomId",
                table: "Showtimes",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.CreateIndex(
                name: "IX_Showtimes_RoomId",
                table: "Showtimes",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Showtimes_Rooms_RoomId",
                table: "Showtimes",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Showtimes_Rooms_RoomId",
                table: "Showtimes");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Showtimes_RoomId",
                table: "Showtimes");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "Showtimes");

            migrationBuilder.AddColumn<string>(
                name: "RoomNumber",
                table: "Showtimes",
                type: "NVARCHAR2(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }
    }
}
