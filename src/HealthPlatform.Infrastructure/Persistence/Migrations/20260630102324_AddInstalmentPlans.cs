using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInstalmentPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "instalment_payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InstalmentPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    AmountMinorUnits = table.Column<long>(type: "bigint", nullable: false),
                    LateFeeMinorUnits = table.Column<long>(type: "bigint", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    DueReminderSent = table.Column<bool>(type: "boolean", nullable: false),
                    PaidAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MissedRecordedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_instalment_payments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "instalment_plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalAmountMinorUnits = table.Column<long>(type: "bigint", nullable: false),
                    InstalmentAmountMinorUnits = table.Column<long>(type: "bigint", nullable: false),
                    TotalRepayableMinorUnits = table.Column<long>(type: "bigint", nullable: false),
                    Frequency = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    TotalInstalments = table.Column<int>(type: "integer", nullable: false),
                    PaidInstalments = table.Column<int>(type: "integer", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    MedicationOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    LabOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    FirstDueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_instalment_plans", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_instalment_payments_InstalmentPlanId",
                table: "instalment_payments",
                column: "InstalmentPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_instalment_payments_InstalmentPlanId_SequenceNumber",
                table: "instalment_payments",
                columns: new[] { "InstalmentPlanId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_instalment_payments_PatientId",
                table: "instalment_payments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_instalment_payments_Status_DueReminderSent_DueDate",
                table: "instalment_payments",
                columns: new[] { "Status", "DueReminderSent", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_instalment_plans_PatientId",
                table: "instalment_plans",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "instalment_payments");

            migrationBuilder.DropTable(
                name: "instalment_plans");
        }
    }
}
