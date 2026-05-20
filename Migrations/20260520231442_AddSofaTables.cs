using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarWash.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSofaTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SofaServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DurationInMinutes = table.Column<int>(type: "int", nullable: false),
                    DurationDisplay = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Includes = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "[]"),
                    IsPopular = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SofaServices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SofaBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerPhone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    CustomerAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SofaType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SofaCount = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledTime = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "pending"),
                    TotalAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "pending"),
                    SpecialInstructions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SelectedServices = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "[]"),
                    SofaServiceId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SofaBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SofaBookings_SofaServices_SofaServiceId",
                        column: x => x.SofaServiceId,
                        principalTable: "SofaServices",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SofaBookings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SofaBookings_BookingId",
                table: "SofaBookings",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SofaBookings_CreatedAt",
                table: "SofaBookings",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SofaBookings_ScheduledDate",
                table: "SofaBookings",
                column: "ScheduledDate");

            migrationBuilder.CreateIndex(
                name: "IX_SofaBookings_SofaServiceId",
                table: "SofaBookings",
                column: "SofaServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_SofaBookings_Status",
                table: "SofaBookings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SofaBookings_UserId",
                table: "SofaBookings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SofaServices_Category",
                table: "SofaServices",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_SofaServices_DisplayOrder",
                table: "SofaServices",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_SofaServices_IsActive",
                table: "SofaServices",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SofaServices_IsPopular",
                table: "SofaServices",
                column: "IsPopular");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SofaBookings");

            migrationBuilder.DropTable(
                name: "SofaServices");
        }
    }
}
