using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarWash.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAcServiceTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ACServices",
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
                    table.PrimaryKey("PK_ACServices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ACBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerPhone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    CustomerAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ACType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ACBrand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ACCapacity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UsageType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
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
                    ACServiceId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ACBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ACBookings_ACServices_ACServiceId",
                        column: x => x.ACServiceId,
                        principalTable: "ACServices",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ACBookings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ACBookings_ACServiceId",
                table: "ACBookings",
                column: "ACServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ACBookings_BookingId",
                table: "ACBookings",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ACBookings_CreatedAt",
                table: "ACBookings",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ACBookings_PaymentStatus",
                table: "ACBookings",
                column: "PaymentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ACBookings_ScheduledDate",
                table: "ACBookings",
                column: "ScheduledDate");

            migrationBuilder.CreateIndex(
                name: "IX_ACBookings_Status",
                table: "ACBookings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ACBookings_UserId",
                table: "ACBookings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ACServices_Category",
                table: "ACServices",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_ACServices_DisplayOrder",
                table: "ACServices",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_ACServices_IsActive",
                table: "ACServices",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ACServices_IsPopular",
                table: "ACServices",
                column: "IsPopular");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ACBookings");

            migrationBuilder.DropTable(
                name: "ACServices");
        }
    }
}
