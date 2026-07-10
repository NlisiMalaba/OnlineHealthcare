using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBirthPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "birth_plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    AntenatalRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_birth_plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maternal_care_access_grants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AntenatalRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShareAntenatalRecord = table.Column<bool>(type: "boolean", nullable: false),
                    ShareBirthPlan = table.Column<bool>(type: "boolean", nullable: false),
                    GrantedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maternal_care_access_grants", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_birth_plans_AntenatalRecordId",
                table: "birth_plans",
                column: "AntenatalRecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_birth_plans_PatientId",
                table: "birth_plans",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_maternal_care_access_grants_AntenatalRecordId_DoctorId_RevokedAtUtc",
                table: "maternal_care_access_grants",
                columns: new[] { "AntenatalRecordId", "DoctorId", "RevokedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_maternal_care_access_grants_PatientId",
                table: "maternal_care_access_grants",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "birth_plans");

            migrationBuilder.DropTable(
                name: "maternal_care_access_grants");
        }
    }
}
