using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHealthRecordAccessAndAuditLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorType = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Action = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ResourceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    TimestampUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MetadataJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "health_record_accesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HealthRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    GrantedToDoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    GrantedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AccessType = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Sections = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_health_record_accesses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_ActorId_TimestampUtc",
                table: "audit_logs",
                columns: new[] { "ActorId", "TimestampUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_ResourceType",
                table: "audit_logs",
                column: "ResourceType");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_TimestampUtc",
                table: "audit_logs",
                column: "TimestampUtc");

            migrationBuilder.CreateIndex(
                name: "IX_health_record_accesses_HealthRecordId",
                table: "health_record_accesses",
                column: "HealthRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_health_record_accesses_HealthRecordId_GrantedToDoctorId_Rev~",
                table: "health_record_accesses",
                columns: new[] { "HealthRecordId", "GrantedToDoctorId", "RevokedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "health_record_accesses");
        }
    }
}
