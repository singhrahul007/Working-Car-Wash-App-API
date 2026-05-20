using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarWash.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixSofaServiceNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SofaBookings_SofaServices_SofaServiceId",
                table: "SofaBookings");

            migrationBuilder.DropIndex(
                name: "IX_SofaBookings_SofaServiceId",
                table: "SofaBookings");

            migrationBuilder.DropColumn(
                name: "SofaServiceId",
                table: "SofaBookings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SofaServiceId",
                table: "SofaBookings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SofaBookings_SofaServiceId",
                table: "SofaBookings",
                column: "SofaServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_SofaBookings_SofaServices_SofaServiceId",
                table: "SofaBookings",
                column: "SofaServiceId",
                principalTable: "SofaServices",
                principalColumn: "Id");
        }
    }
}
