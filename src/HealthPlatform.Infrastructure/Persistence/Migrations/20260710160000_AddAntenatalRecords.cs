using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAntenatalRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "antenatal_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    EstimatedDueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    GestationalAgeWeeks = table.Column<int>(type: "integer", nullable: false),
                    ObstetricDoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    EntryRefs = table.Column<string>(type: "jsonb", nullable: false),
                    NextReminderAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastReminderSentAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_antenatal_records", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "antenatal_checkup_schedule_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AntenatalRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    GestationalAgeWeeks = table.Column<int>(type: "integer", nullable: false),
                    RecommendedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_antenatal_checkup_schedule_entries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_antenatal_records_NextReminderAtUtc",
                table: "antenatal_records",
                column: "NextReminderAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_antenatal_records_ObstetricDoctorId",
                table: "antenatal_records",
                column: "ObstetricDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_antenatal_records_PatientId",
                table: "antenatal_records",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_antenatal_records_PatientId_Status",
                table: "antenatal_records",
                columns: new[] { "PatientId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_antenatal_checkup_schedule_entries_AntenatalRecordId",
                table: "antenatal_checkup_schedule_entries",
                column: "AntenatalRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_antenatal_checkup_schedule_entries_AntenatalRecordId_RecommendedDate",
                table: "antenatal_checkup_schedule_entries",
                columns: new[] { "AntenatalRecordId", "RecommendedDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "antenatal_checkup_schedule_entries");

            migrationBuilder.DropTable(
                name: "antenatal_records");
        }
    }
}
