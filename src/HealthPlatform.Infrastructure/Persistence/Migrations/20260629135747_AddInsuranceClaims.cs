using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInsuranceClaims : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "insurance_claims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientInsurancePolicyId = table.Column<Guid>(type: "uuid", nullable: false),
                    InsurerCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ClaimType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    MedicationOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    LabOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    AmountMinorUnits = table.Column<long>(type: "bigint", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    InsurerClaimReference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    StatusReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SubmittedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastStatusCheckedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_insurance_claims", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "patient_insurance_policies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    InsurerCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PolicyNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    MemberNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patient_insurance_policies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_insurance_claims_InsurerCode_InsurerClaimReference",
                table: "insurance_claims",
                columns: new[] { "InsurerCode", "InsurerClaimReference" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_insurance_claims_PatientId",
                table: "insurance_claims",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_insurance_claims_PatientId_ClaimType_AppointmentId_Medicati~",
                table: "insurance_claims",
                columns: new[] { "PatientId", "ClaimType", "AppointmentId", "MedicationOrderId", "LabOrderId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_patient_insurance_policies_PatientId",
                table: "patient_insurance_policies",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_patient_insurance_policies_PatientId_InsurerCode_PolicyNumb~",
                table: "patient_insurance_policies",
                columns: new[] { "PatientId", "InsurerCode", "PolicyNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "insurance_claims");

            migrationBuilder.DropTable(
                name: "patient_insurance_policies");
        }
    }
}
