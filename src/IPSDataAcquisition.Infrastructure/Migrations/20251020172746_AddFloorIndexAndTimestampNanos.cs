using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPSDataAcquisition.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFloorIndexAndTimestampNanos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "timestamp_nanos",
                table: "imu_data",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "floor_index",
                table: "button_presses",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "timestamp_nanos",
                table: "imu_data");

            migrationBuilder.DropColumn(
                name: "floor_index",
                table: "button_presses");
        }
    }
}
