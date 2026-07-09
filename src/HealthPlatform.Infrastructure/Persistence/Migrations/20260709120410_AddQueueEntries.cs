using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQueueEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "lab_orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    HealthRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderingDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestSource = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    LabPartnerCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TestCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ClinicalNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LabPartnerOrderReference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ApprovedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lab_orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "lab_results",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LabOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    HealthRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderingDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LabPartnerCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LabPartnerOrderReference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TestCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    IsCritical = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lab_results", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "queue_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AppointmentScheduledAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    QueuePosition = table.Column<int>(type: "integer", nullable: false),
                    EstimatedWaitMinutes = table.Column<int>(type: "integer", nullable: false),
                    ArrivalStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ActualWaitMinutes = table.Column<int>(type: "integer", nullable: true),
                    JoinedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_queue_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "radiology_reports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LabOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    HealthRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderingDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LabPartnerCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LabPartnerOrderReference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ReportStorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ReportContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ReportFileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    ImagingStorageKeysJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_radiology_reports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "referral_health_record_access_grants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferralId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    SharedHealthRecordSections = table.Column<string>(type: "jsonb", nullable: false),
                    GrantedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_referral_health_record_access_grants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "referrals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferringDoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivingDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceivingHospitalName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ClinicalNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SharedHealthRecordSections = table.Column<string>(type: "jsonb", nullable: false),
                    PatientConsentAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RespondedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResponseReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ConsultationSummaryEntryId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TimeoutReminderSentAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_referrals", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_lab_orders_HealthRecordId",
                table: "lab_orders",
                column: "HealthRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_lab_orders_PatientId",
                table: "lab_orders",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_lab_results_LabOrderId",
                table: "lab_results",
                column: "LabOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_lab_results_LabPartnerCode_LabPartnerOrderReference",
                table: "lab_results",
                columns: new[] { "LabPartnerCode", "LabPartnerOrderReference" });

            migrationBuilder.CreateIndex(
                name: "IX_lab_results_PatientId",
                table: "lab_results",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_queue_entries_AppointmentId",
                table: "queue_entries",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_queue_entries_DoctorId_ArrivalStatus",
                table: "queue_entries",
                columns: new[] { "DoctorId", "ArrivalStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_queue_entries_PatientId",
                table: "queue_entries",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_radiology_reports_LabOrderId",
                table: "radiology_reports",
                column: "LabOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_radiology_reports_LabPartnerCode_LabPartnerOrderReference",
                table: "radiology_reports",
                columns: new[] { "LabPartnerCode", "LabPartnerOrderReference" });

            migrationBuilder.CreateIndex(
                name: "IX_radiology_reports_PatientId",
                table: "radiology_reports",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_referral_health_record_access_grants_DoctorId",
                table: "referral_health_record_access_grants",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_referral_health_record_access_grants_PatientId",
                table: "referral_health_record_access_grants",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_referral_health_record_access_grants_ReferralId",
                table: "referral_health_record_access_grants",
                column: "ReferralId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_referrals_PatientId",
                table: "referrals",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_referrals_PatientId_Status",
                table: "referrals",
                columns: new[] { "PatientId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_referrals_ReceivingDoctorId",
                table: "referrals",
                column: "ReceivingDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_referrals_ReferringDoctorId",
                table: "referrals",
                column: "ReferringDoctorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lab_orders");

            migrationBuilder.DropTable(
                name: "lab_results");

            migrationBuilder.DropTable(
                name: "queue_entries");

            migrationBuilder.DropTable(
                name: "radiology_reports");

            migrationBuilder.DropTable(
                name: "referral_health_record_access_grants");

            migrationBuilder.DropTable(
                name: "referrals");
        }
    }
}
