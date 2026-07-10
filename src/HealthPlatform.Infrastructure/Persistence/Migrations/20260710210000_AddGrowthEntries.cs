using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGrowthEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "growth_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChildProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    HeightCm = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    WeightKg = table.Column<decimal>(type: "numeric(6,3)", precision: 6, scale: 3, nullable: true),
                    MilestoneNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RecordedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_growth_entries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_growth_entries_ChildProfileId",
                table: "growth_entries",
                column: "ChildProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_growth_entries_ChildProfileId_RecordedAtUtc",
                table: "growth_entries",
                columns: new[] { "ChildProfileId", "RecordedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "growth_entries");
        }
    }
}
