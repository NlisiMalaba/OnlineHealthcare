using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAntenatalCheckupRecording : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAtUtc",
                table: "antenatal_checkup_schedule_entries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckupEntryRef",
                table: "antenatal_checkup_schedule_entries",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FetalMonitoringReminderIntervalDays",
                table: "antenatal_records",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastFetalMonitoringReminderSentAtUtc",
                table: "antenatal_records",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextFetalMonitoringReminderAtUtc",
                table: "antenatal_records",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_antenatal_records_NextFetalMonitoringReminderAtUtc",
                table: "antenatal_records",
                column: "NextFetalMonitoringReminderAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_antenatal_records_NextFetalMonitoringReminderAtUtc",
                table: "antenatal_records");

            migrationBuilder.DropColumn(
                name: "CompletedAtUtc",
                table: "antenatal_checkup_schedule_entries");

            migrationBuilder.DropColumn(
                name: "CheckupEntryRef",
                table: "antenatal_checkup_schedule_entries");

            migrationBuilder.DropColumn(
                name: "FetalMonitoringReminderIntervalDays",
                table: "antenatal_records");

            migrationBuilder.DropColumn(
                name: "LastFetalMonitoringReminderSentAtUtc",
                table: "antenatal_records");

            migrationBuilder.DropColumn(
                name: "NextFetalMonitoringReminderAtUtc",
                table: "antenatal_records");
        }
    }
}
