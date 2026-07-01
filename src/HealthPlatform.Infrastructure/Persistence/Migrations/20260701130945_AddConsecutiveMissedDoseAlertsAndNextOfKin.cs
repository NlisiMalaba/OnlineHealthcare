using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConsecutiveMissedDoseAlertsAndNextOfKin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "consecutive_missed_dose_alerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    TriggeringAdherenceEventId = table.Column<Guid>(type: "uuid", nullable: false),
                    StreakEndScheduledAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TriggeredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consecutive_missed_dose_alerts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "next_of_kin_contacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Relationship = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    IsMentalHealthContact = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_next_of_kin_contacts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_consecutive_missed_dose_alerts_PatientId_StreakEndScheduled~",
                table: "consecutive_missed_dose_alerts",
                columns: new[] { "PatientId", "StreakEndScheduledAtUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_consecutive_missed_dose_alerts_TriggeringAdherenceEventId",
                table: "consecutive_missed_dose_alerts",
                column: "TriggeringAdherenceEventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_next_of_kin_contacts_PatientId",
                table: "next_of_kin_contacts",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "consecutive_missed_dose_alerts");

            migrationBuilder.DropTable(
                name: "next_of_kin_contacts");
        }
    }
}
