using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicationDoseReminders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "medication_dose_reminders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduledAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SentAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medication_dose_reminders", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_medication_dose_reminders_PatientId",
                table: "medication_dose_reminders",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_medication_dose_reminders_ScheduledAtUtc",
                table: "medication_dose_reminders",
                column: "ScheduledAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_medication_dose_reminders_ScheduleId_ScheduledAtUtc",
                table: "medication_dose_reminders",
                columns: new[] { "ScheduleId", "ScheduledAtUtc" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "medication_dose_reminders");
        }
    }
}
