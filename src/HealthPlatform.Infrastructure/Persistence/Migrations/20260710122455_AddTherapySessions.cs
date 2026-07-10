using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTherapySessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConsultationType",
                table: "appointments",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "therapy_sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    TherapistId = table.Column<Guid>(type: "uuid", nullable: false),
                    SummaryRef = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SummaryEntryId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    IsVisibleToPatient = table.Column<bool>(type: "boolean", nullable: false),
                    BroaderAccessGranted = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_therapy_sessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_therapy_sessions_AppointmentId",
                table: "therapy_sessions",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_therapy_sessions_PatientId",
                table: "therapy_sessions",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_therapy_sessions_TherapistId",
                table: "therapy_sessions",
                column: "TherapistId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "therapy_sessions");

            migrationBuilder.DropColumn(
                name: "ConsultationType",
                table: "appointments");
        }
    }
}
