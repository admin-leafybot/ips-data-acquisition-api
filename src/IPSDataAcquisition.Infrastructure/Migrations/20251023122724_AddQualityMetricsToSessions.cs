using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPSDataAcquisition.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQualityMetricsToSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "accel_data_coverage",
                table: "sessions",
                type: "numeric(5,2)",
                nullable: true,
                comment: "Percentage of records with accelerometer data (0-100)");

            migrationBuilder.AddColumn<decimal>(
                name: "barometer_data_coverage",
                table: "sessions",
                type: "numeric(5,2)",
                nullable: true,
                comment: "Percentage of records with barometer/pressure data (0-100)");

            migrationBuilder.AddColumn<int>(
                name: "data_gap_count",
                table: "sessions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Number of data gaps detected (>1 second)");

            migrationBuilder.AddColumn<double>(
                name: "duration_minutes",
                table: "sessions",
                type: "double precision",
                nullable: true,
                comment: "Session duration in minutes");

            migrationBuilder.AddColumn<decimal>(
                name: "gps_data_coverage",
                table: "sessions",
                type: "numeric(5,2)",
                nullable: true,
                comment: "Percentage of records with GPS data (0-100)");

            migrationBuilder.AddColumn<decimal>(
                name: "gyro_data_coverage",
                table: "sessions",
                type: "numeric(5,2)",
                nullable: true,
                comment: "Percentage of records with gyroscope data (0-100)");

            migrationBuilder.AddColumn<bool>(
                name: "has_anomalies",
                table: "sessions",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Flag indicating if sensor anomalies were detected");

            migrationBuilder.AddColumn<bool>(
                name: "has_data_gaps",
                table: "sessions",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Flag indicating if data gaps were detected");

            migrationBuilder.AddColumn<decimal>(
                name: "mag_data_coverage",
                table: "sessions",
                type: "numeric(5,2)",
                nullable: true,
                comment: "Percentage of records with magnetometer data (0-100)");

            migrationBuilder.AddColumn<DateTime>(
                name: "quality_checked_at",
                table: "sessions",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Timestamp when quality check was performed");

            migrationBuilder.AddColumn<string>(
                name: "quality_metrics_raw_json",
                table: "sessions",
                type: "jsonb",
                nullable: true,
                comment: "Extended quality metrics and ML features in JSON format");

            migrationBuilder.AddColumn<string>(
                name: "quality_remarks",
                table: "sessions",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                comment: "Human-readable quality issues/notes");

            migrationBuilder.AddColumn<decimal>(
                name: "quality_score",
                table: "sessions",
                type: "numeric(5,2)",
                nullable: true,
                comment: "Overall quality score (0-100)");

            migrationBuilder.AddColumn<int>(
                name: "quality_status",
                table: "sessions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Quality check status: 0=pending, 1=completed, 2=failed");

            migrationBuilder.AddColumn<int>(
                name: "total_button_presses",
                table: "sessions",
                type: "integer",
                nullable: true,
                comment: "Total number of button press events");

            migrationBuilder.AddColumn<int>(
                name: "total_imu_data_points",
                table: "sessions",
                type: "integer",
                nullable: true,
                comment: "Total number of IMU data points");

            migrationBuilder.CreateIndex(
                name: "idx_sessions_quality_analysis",
                table: "sessions",
                columns: new[] { "quality_score", "has_anomalies", "status" });

            migrationBuilder.CreateIndex(
                name: "idx_sessions_quality_score",
                table: "sessions",
                column: "quality_score");

            migrationBuilder.CreateIndex(
                name: "idx_sessions_quality_status",
                table: "sessions",
                column: "quality_status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_sessions_quality_analysis",
                table: "sessions");

            migrationBuilder.DropIndex(
                name: "idx_sessions_quality_score",
                table: "sessions");

            migrationBuilder.DropIndex(
                name: "idx_sessions_quality_status",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "accel_data_coverage",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "barometer_data_coverage",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "data_gap_count",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "duration_minutes",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "gps_data_coverage",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "gyro_data_coverage",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "has_anomalies",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "has_data_gaps",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "mag_data_coverage",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "quality_checked_at",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "quality_metrics_raw_json",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "quality_remarks",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "quality_score",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "quality_status",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "total_button_presses",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "total_imu_data_points",
                table: "sessions");
        }
    }
}
