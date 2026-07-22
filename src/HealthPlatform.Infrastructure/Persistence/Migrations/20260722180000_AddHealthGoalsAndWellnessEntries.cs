using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHealthGoalsAndWellnessEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "health_goals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    MetricType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TargetValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Unit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CustomLabel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_health_goals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "wellness_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalId = table.Column<Guid>(type: "uuid", nullable: true),
                    MetricType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Value = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    RecordedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wellness_entries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_health_goals_PatientId",
                table: "health_goals",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_health_goals_PatientId_MetricType_Status",
                table: "health_goals",
                columns: new[] { "PatientId", "MetricType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_health_goals_PatientId_Status",
                table: "health_goals",
                columns: new[] { "PatientId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_wellness_entries_GoalId",
                table: "wellness_entries",
                column: "GoalId");

            migrationBuilder.CreateIndex(
                name: "IX_wellness_entries_PatientId",
                table: "wellness_entries",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_wellness_entries_PatientId_MetricType_RecordedAtUtc",
                table: "wellness_entries",
                columns: new[] { "PatientId", "MetricType", "RecordedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "health_goals");

            migrationBuilder.DropTable(
                name: "wellness_entries");
        }
    }
}
