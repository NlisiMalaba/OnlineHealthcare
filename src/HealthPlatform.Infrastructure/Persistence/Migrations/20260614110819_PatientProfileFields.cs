using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PatientProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BloodType",
                table: "patients",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "ChronicConditions",
                table: "patients",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfBirth",
                table: "patients",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "KnownAllergies",
                table: "patients",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePhotoStorageKey",
                table: "patients",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "health_record_profile_changes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HealthRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PreviousValue = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    NewValue = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ChangedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_health_record_profile_changes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_health_record_profile_changes_HealthRecordId",
                table: "health_record_profile_changes",
                column: "HealthRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_health_record_profile_changes_PatientId_ChangedAtUtc",
                table: "health_record_profile_changes",
                columns: new[] { "PatientId", "ChangedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "health_record_profile_changes");

            migrationBuilder.DropColumn(
                name: "BloodType",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "ChronicConditions",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "KnownAllergies",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "ProfilePhotoStorageKey",
                table: "patients");
        }
    }
}
