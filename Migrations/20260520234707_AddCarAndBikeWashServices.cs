using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarWash.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCarAndBikeWashServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BikeWashBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerPhone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    CustomerAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    BikeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledTime = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "pending"),
                    TotalAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "pending"),
                    SpecialInstructions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SelectedServices = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "[]")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BikeWashBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BikeWashBookings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BikeWashServices",
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
                    table.PrimaryKey("PK_BikeWashServices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CarWashBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerPhone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    CustomerAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    VehicleSize = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledTime = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "pending"),
                    TotalAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "pending"),
                    SpecialInstructions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SelectedServices = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "[]")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarWashBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarWashBookings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CarWashServices",
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
                    table.PrimaryKey("PK_CarWashServices", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BikeWashBookings_BookingId",
                table: "BikeWashBookings",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BikeWashBookings_CreatedAt",
                table: "BikeWashBookings",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BikeWashBookings_ScheduledDate",
                table: "BikeWashBookings",
                column: "ScheduledDate");

            migrationBuilder.CreateIndex(
                name: "IX_BikeWashBookings_Status",
                table: "BikeWashBookings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BikeWashBookings_UserId",
                table: "BikeWashBookings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BikeWashServices_Category",
                table: "BikeWashServices",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_BikeWashServices_DisplayOrder",
                table: "BikeWashServices",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_BikeWashServices_IsActive",
                table: "BikeWashServices",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_BikeWashServices_IsPopular",
                table: "BikeWashServices",
                column: "IsPopular");

            migrationBuilder.CreateIndex(
                name: "IX_CarWashBookings_BookingId",
                table: "CarWashBookings",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CarWashBookings_CreatedAt",
                table: "CarWashBookings",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CarWashBookings_ScheduledDate",
                table: "CarWashBookings",
                column: "ScheduledDate");

            migrationBuilder.CreateIndex(
                name: "IX_CarWashBookings_Status",
                table: "CarWashBookings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CarWashBookings_UserId",
                table: "CarWashBookings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CarWashServices_Category",
                table: "CarWashServices",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_CarWashServices_DisplayOrder",
                table: "CarWashServices",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_CarWashServices_IsActive",
                table: "CarWashServices",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CarWashServices_IsPopular",
                table: "CarWashServices",
                column: "IsPopular");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BikeWashBookings");

            migrationBuilder.DropTable(
                name: "BikeWashServices");

            migrationBuilder.DropTable(
                name: "CarWashBookings");

            migrationBuilder.DropTable(
                name: "CarWashServices");
        }
    }
}
