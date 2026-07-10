using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddChildProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_health_records_PatientId",
                table: "health_records");

            migrationBuilder.AddColumn<Guid>(
                name: "ChildProfileId",
                table: "health_records",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "child_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GuardianId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    BloodType = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    KnownAllergies = table.Column<string[]>(type: "text[]", nullable: false),
                    HealthRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_child_profiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_health_records_ChildProfileId",
                table: "health_records",
                column: "ChildProfileId",
                unique: true,
                filter: "\"ChildProfileId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_health_records_PatientId",
                table: "health_records",
                column: "PatientId",
                unique: true,
                filter: "\"ChildProfileId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_child_profiles_GuardianId",
                table: "child_profiles",
                column: "GuardianId");

            migrationBuilder.CreateIndex(
                name: "IX_child_profiles_HealthRecordId",
                table: "child_profiles",
                column: "HealthRecordId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "child_profiles");

            migrationBuilder.DropIndex(
                name: "IX_health_records_ChildProfileId",
                table: "health_records");

            migrationBuilder.DropIndex(
                name: "IX_health_records_PatientId",
                table: "health_records");

            migrationBuilder.DropColumn(
                name: "ChildProfileId",
                table: "health_records");

            migrationBuilder.CreateIndex(
                name: "IX_health_records_PatientId",
                table: "health_records",
                column: "PatientId",
                unique: true);
        }
    }
}
