using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmergencyAlerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "emergency_alert_contact_deliveries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmergencyAlertId = table.Column<Guid>(type: "uuid", nullable: false),
                    NextOfKinContactId = table.Column<Guid>(type: "uuid", nullable: false),
                    SmsStatus = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    PushStatus = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_emergency_alert_contact_deliveries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "emergency_alerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    TriggerSource = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TriggeredByDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    TriggerReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TriggeredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OverallStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_emergency_alerts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_emergency_alert_contact_deliveries_EmergencyAlertId",
                table: "emergency_alert_contact_deliveries",
                column: "EmergencyAlertId");

            migrationBuilder.CreateIndex(
                name: "IX_emergency_alert_contact_deliveries_EmergencyAlertId_NextOfK~",
                table: "emergency_alert_contact_deliveries",
                columns: new[] { "EmergencyAlertId", "NextOfKinContactId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_emergency_alerts_PatientId",
                table: "emergency_alerts",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_emergency_alerts_TriggeredAtUtc",
                table: "emergency_alerts",
                column: "TriggeredAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "emergency_alert_contact_deliveries");

            migrationBuilder.DropTable(
                name: "emergency_alerts");
        }
    }
}
