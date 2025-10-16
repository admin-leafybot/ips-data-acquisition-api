using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IPSDataAcquisition.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bonuses",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    sessions_completed = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bonuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sessions",
                columns: table => new
                {
                    session_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    user_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    start_timestamp = table.Column<long>(type: "bigint", nullable: false),
                    end_timestamp = table.Column<long>(type: "bigint", nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    payment_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    bonus_amount = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sessions", x => x.session_id);
                });

            migrationBuilder.CreateTable(
                name: "button_presses",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    session_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    user_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    timestamp = table.Column<long>(type: "bigint", nullable: false),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_button_presses", x => x.id);
                    table.ForeignKey(
                        name: "fk_button_presses_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "sessions",
                        principalColumn: "session_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "imu_data",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    session_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    user_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    timestamp = table.Column<long>(type: "bigint", nullable: false),
                    accel_x = table.Column<float>(type: "real", nullable: true),
                    accel_y = table.Column<float>(type: "real", nullable: true),
                    accel_z = table.Column<float>(type: "real", nullable: true),
                    gyro_x = table.Column<float>(type: "real", nullable: true),
                    gyro_y = table.Column<float>(type: "real", nullable: true),
                    gyro_z = table.Column<float>(type: "real", nullable: true),
                    mag_x = table.Column<float>(type: "real", nullable: true),
                    mag_y = table.Column<float>(type: "real", nullable: true),
                    mag_z = table.Column<float>(type: "real", nullable: true),
                    gravity_x = table.Column<float>(type: "real", nullable: true),
                    gravity_y = table.Column<float>(type: "real", nullable: true),
                    gravity_z = table.Column<float>(type: "real", nullable: true),
                    linear_accel_x = table.Column<float>(type: "real", nullable: true),
                    linear_accel_y = table.Column<float>(type: "real", nullable: true),
                    linear_accel_z = table.Column<float>(type: "real", nullable: true),
                    accel_uncal_x = table.Column<float>(type: "real", nullable: true),
                    accel_uncal_y = table.Column<float>(type: "real", nullable: true),
                    accel_uncal_z = table.Column<float>(type: "real", nullable: true),
                    accel_bias_x = table.Column<float>(type: "real", nullable: true),
                    accel_bias_y = table.Column<float>(type: "real", nullable: true),
                    accel_bias_z = table.Column<float>(type: "real", nullable: true),
                    gyro_uncal_x = table.Column<float>(type: "real", nullable: true),
                    gyro_uncal_y = table.Column<float>(type: "real", nullable: true),
                    gyro_uncal_z = table.Column<float>(type: "real", nullable: true),
                    gyro_drift_x = table.Column<float>(type: "real", nullable: true),
                    gyro_drift_y = table.Column<float>(type: "real", nullable: true),
                    gyro_drift_z = table.Column<float>(type: "real", nullable: true),
                    mag_uncal_x = table.Column<float>(type: "real", nullable: true),
                    mag_uncal_y = table.Column<float>(type: "real", nullable: true),
                    mag_uncal_z = table.Column<float>(type: "real", nullable: true),
                    mag_bias_x = table.Column<float>(type: "real", nullable: true),
                    mag_bias_y = table.Column<float>(type: "real", nullable: true),
                    mag_bias_z = table.Column<float>(type: "real", nullable: true),
                    rotation_vector_x = table.Column<float>(type: "real", nullable: true),
                    rotation_vector_y = table.Column<float>(type: "real", nullable: true),
                    rotation_vector_z = table.Column<float>(type: "real", nullable: true),
                    rotation_vector_w = table.Column<float>(type: "real", nullable: true),
                    game_rotation_x = table.Column<float>(type: "real", nullable: true),
                    game_rotation_y = table.Column<float>(type: "real", nullable: true),
                    game_rotation_z = table.Column<float>(type: "real", nullable: true),
                    game_rotation_w = table.Column<float>(type: "real", nullable: true),
                    geomag_rotation_x = table.Column<float>(type: "real", nullable: true),
                    geomag_rotation_y = table.Column<float>(type: "real", nullable: true),
                    geomag_rotation_z = table.Column<float>(type: "real", nullable: true),
                    geomag_rotation_w = table.Column<float>(type: "real", nullable: true),
                    pressure = table.Column<float>(type: "real", nullable: true),
                    temperature = table.Column<float>(type: "real", nullable: true),
                    light = table.Column<float>(type: "real", nullable: true),
                    humidity = table.Column<float>(type: "real", nullable: true),
                    proximity = table.Column<float>(type: "real", nullable: true),
                    step_counter = table.Column<int>(type: "integer", nullable: true),
                    step_detected = table.Column<bool>(type: "boolean", nullable: true),
                    roll = table.Column<float>(type: "real", nullable: true),
                    pitch = table.Column<float>(type: "real", nullable: true),
                    yaw = table.Column<float>(type: "real", nullable: true),
                    heading = table.Column<float>(type: "real", nullable: true),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    altitude = table.Column<double>(type: "double precision", nullable: true),
                    gps_accuracy = table.Column<float>(type: "real", nullable: true),
                    speed = table.Column<float>(type: "real", nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_imu_data", x => x.id);
                    table.ForeignKey(
                        name: "fk_imu_data_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "sessions",
                        principalColumn: "session_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_bonuses_date",
                table: "bonuses",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "ix_bonuses_user_id",
                table: "bonuses",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_bonuses_user_id_date",
                table: "bonuses",
                columns: new[] { "user_id", "date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_button_presses_session_id",
                table: "button_presses",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "ix_button_presses_timestamp",
                table: "button_presses",
                column: "timestamp");

            migrationBuilder.CreateIndex(
                name: "ix_button_presses_user_id",
                table: "button_presses",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_imu_data_session_id_timestamp",
                table: "imu_data",
                columns: new[] { "session_id", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "ix_imu_data_user_id",
                table: "imu_data",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_sessions_start_timestamp",
                table: "sessions",
                column: "start_timestamp");

            migrationBuilder.CreateIndex(
                name: "ix_sessions_status",
                table: "sessions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_sessions_user_id",
                table: "sessions",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bonuses");

            migrationBuilder.DropTable(
                name: "button_presses");

            migrationBuilder.DropTable(
                name: "imu_data");

            migrationBuilder.DropTable(
                name: "sessions");
        }
    }
}
