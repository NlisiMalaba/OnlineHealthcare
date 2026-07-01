using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicationScheduleDoseTimes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime[]>(
                name: "DoseTimes",
                table: "medication_schedules",
                type: "timestamp with time zone[]",
                nullable: false,
                defaultValue: new DateTime[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DoseTimes",
                table: "medication_schedules");
        }
    }
}
