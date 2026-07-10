using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVaccinationRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "vaccination_schedule_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChildProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: true),
                    VaccineName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RecommendedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VaccinationRecordId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReminderSentAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vaccination_schedule_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "vaccination_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChildProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScheduleEntryId = table.Column<Guid>(type: "uuid", nullable: true),
                    VaccineName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AdministeredDate = table.Column<DateOnly>(type: "date", nullable: false),
                    BatchNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Provider = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vaccination_records", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_vaccination_schedule_entries_ChildProfileId",
                table: "vaccination_schedule_entries",
                column: "ChildProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_vaccination_schedule_entries_PatientId",
                table: "vaccination_schedule_entries",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_vaccination_schedule_entries_RecommendedDate_ReminderSentAtUtc_CompletedAtUtc",
                table: "vaccination_schedule_entries",
                columns: new[] { "RecommendedDate", "ReminderSentAtUtc", "CompletedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_vaccination_records_ChildProfileId",
                table: "vaccination_records",
                column: "ChildProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_vaccination_records_PatientId",
                table: "vaccination_records",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_vaccination_records_ScheduleEntryId",
                table: "vaccination_records",
                column: "ScheduleEntryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vaccination_schedule_entries");

            migrationBuilder.DropTable(
                name: "vaccination_records");
        }
    }
}
