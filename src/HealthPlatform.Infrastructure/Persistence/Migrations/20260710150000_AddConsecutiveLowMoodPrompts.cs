using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConsecutiveLowMoodPrompts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "consecutive_low_mood_prompts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    TriggeringMoodLogId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StreakEndLoggedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TriggeredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consecutive_low_mood_prompts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_consecutive_low_mood_prompts_PatientId",
                table: "consecutive_low_mood_prompts",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_consecutive_low_mood_prompts_PatientId_TriggeringMoodLogId",
                table: "consecutive_low_mood_prompts",
                columns: new[] { "PatientId", "TriggeringMoodLogId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "consecutive_low_mood_prompts");
        }
    }
}
