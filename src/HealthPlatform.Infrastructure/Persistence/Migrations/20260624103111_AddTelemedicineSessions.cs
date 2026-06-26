using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTelemedicineSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "telemedicine_sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChannelName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RtcProvider = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Mode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    RecordingConsent = table.Column<bool>(type: "boolean", nullable: false),
                    RecordingEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    RecordingUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: false),
                    SessionSummaryRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_telemedicine_sessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_telemedicine_sessions_AppointmentId",
                table: "telemedicine_sessions",
                column: "AppointmentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "telemedicine_sessions");
        }
    }
}
