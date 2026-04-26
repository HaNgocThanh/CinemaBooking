using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CinemaBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomAndSeatTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    DisplayOrder = table.Column<int>(type: "NUMBER(10)", nullable: false)
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
                columns: new[] { "Id", "DisplayOrder", "Number", "RoomId", "Row", "Type" },
                values: new object[,]
                {
                    { 1, 1, 1, 1, "A", 0 },
                    { 2, 2, 2, 1, "A", 0 },
                    { 3, 3, 3, 1, "A", 0 },
                    { 4, 4, 4, 1, "A", 0 },
                    { 5, 5, 5, 1, "A", 0 },
                    { 6, 6, 1, 1, "B", 0 },
                    { 7, 7, 2, 1, "B", 0 },
                    { 8, 8, 3, 1, "B", 0 },
                    { 9, 9, 4, 1, "B", 0 },
                    { 10, 10, 5, 1, "B", 0 },
                    { 11, 11, 1, 1, "C", 1 },
                    { 12, 12, 2, 1, "C", 1 },
                    { 13, 13, 3, 1, "C", 1 },
                    { 14, 14, 4, 1, "C", 1 },
                    { 15, 15, 5, 1, "C", 1 },
                    { 16, 16, 1, 1, "D", 1 },
                    { 17, 17, 2, 1, "D", 1 },
                    { 18, 18, 3, 1, "D", 1 },
                    { 19, 19, 4, 1, "D", 1 },
                    { 20, 20, 5, 1, "D", 1 },
                    { 21, 1, 1, 2, "A", 0 },
                    { 22, 2, 2, 2, "A", 0 },
                    { 23, 3, 3, 2, "A", 0 },
                    { 24, 4, 4, 2, "A", 0 },
                    { 25, 5, 5, 2, "A", 0 },
                    { 26, 6, 1, 2, "B", 0 },
                    { 27, 7, 2, 2, "B", 0 },
                    { 28, 8, 3, 2, "B", 0 },
                    { 29, 9, 4, 2, "B", 0 },
                    { 30, 10, 5, 2, "B", 0 },
                    { 31, 11, 1, 2, "C", 1 },
                    { 32, 12, 2, 2, "C", 1 },
                    { 33, 13, 3, 2, "C", 1 },
                    { 34, 14, 4, 2, "C", 1 },
                    { 35, 15, 5, 2, "C", 1 },
                    { 36, 16, 1, 2, "D", 1 },
                    { 37, 17, 2, 2, "D", 1 },
                    { 38, 18, 3, 2, "D", 1 },
                    { 39, 19, 4, 2, "D", 1 },
                    { 40, 20, 5, 2, "D", 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SeatTemplate_RoomId",
                table: "SeatTemplates",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "UX_SeatTemplate_RoomId_Row_Number",
                table: "SeatTemplates",
                columns: new[] { "RoomId", "Row", "Number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeatTemplates");

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
