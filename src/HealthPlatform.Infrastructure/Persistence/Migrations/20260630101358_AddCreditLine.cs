using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditLine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "credit_line_transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreditLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionType = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    AmountMinorUnits = table.Column<long>(type: "bigint", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    OutstandingBalanceAfterMinorUnits = table.Column<long>(type: "bigint", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    MedicationOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    LabOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    RepaymentDueAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RepaymentReminderSent = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_credit_line_transactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "patient_credit_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreditLimitMinorUnits = table.Column<long>(type: "bigint", nullable: false),
                    OutstandingBalanceMinorUnits = table.Column<long>(type: "bigint", nullable: false),
                    CreditScore = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patient_credit_lines", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_credit_line_transactions_CreditLineId",
                table: "credit_line_transactions",
                column: "CreditLineId");

            migrationBuilder.CreateIndex(
                name: "IX_credit_line_transactions_PatientId",
                table: "credit_line_transactions",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_credit_line_transactions_RepaymentReminderSent_RepaymentDue~",
                table: "credit_line_transactions",
                columns: new[] { "RepaymentReminderSent", "RepaymentDueAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_patient_credit_lines_PatientId",
                table: "patient_credit_lines",
                column: "PatientId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "credit_line_transactions");

            migrationBuilder.DropTable(
                name: "patient_credit_lines");
        }
    }
}
