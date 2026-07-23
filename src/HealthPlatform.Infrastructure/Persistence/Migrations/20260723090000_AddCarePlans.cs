using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCarePlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "care_plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Condition = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Tasks = table.Column<string>(type: "jsonb", nullable: false),
                    MonitoringTargets = table.Column<string>(type: "jsonb", nullable: false),
                    ReviewIntervalDays = table.Column<int>(type: "integer", nullable: false),
                    NextReviewAt = table.Column<DateOnly>(type: "date", nullable: false),
                    ReviewReminderSentAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_care_plans", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_care_plans_DoctorId",
                table: "care_plans",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_care_plans_DoctorId_Status",
                table: "care_plans",
                columns: new[] { "DoctorId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_care_plans_PatientId",
                table: "care_plans",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_care_plans_PatientId_Status",
                table: "care_plans",
                columns: new[] { "PatientId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_care_plans_Status_NextReviewAt",
                table: "care_plans",
                columns: new[] { "Status", "NextReviewAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "care_plans");
        }
    }
}
