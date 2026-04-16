using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAuthentication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "HoldExpiryTime",
                table: "ShowtimeSeats",
                type: "TIMESTAMP(7)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Email = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    FullName = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: true),
                    Role = table.Column<int>(type: "NUMBER(10)", nullable: false, defaultValue: 0),
                    IsEmailConfirmed = table.Column<bool>(type: "NUMBER(1)", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "NUMBER(1)", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "DATE", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "DATE", nullable: true),
                    LastLogin = table.Column<DateTime>(type: "DATE", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CustomerId",
                table: "Bookings",
                column: "CustomerId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Users_CustomerId",
                table: "Bookings",
                column: "CustomerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Users_CustomerId",
                table: "Bookings");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_CustomerId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "HoldExpiryTime",
                table: "ShowtimeSeats");
        }
    }
}
