using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PharmacyRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorAvailabilitySlots_doctors_DoctorId",
                table: "DoctorAvailabilitySlots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DoctorAvailabilitySlots",
                table: "DoctorAvailabilitySlots");

            migrationBuilder.RenameTable(
                name: "DoctorAvailabilitySlots",
                newName: "doctor_availability_slots");

            migrationBuilder.RenameIndex(
                name: "IX_DoctorAvailabilitySlots_DoctorId",
                table: "doctor_availability_slots",
                newName: "IX_doctor_availability_slots_DoctorId");

            migrationBuilder.AlterColumn<string>(
                name: "AppointmentType",
                table: "doctor_availability_slots",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddPrimaryKey(
                name: "PK_doctor_availability_slots",
                table: "doctor_availability_slots",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "pharmacies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    ContactEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ContactPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LogoStorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    VerificationStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pharmacies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_pharmacies_ContactEmail",
                table: "pharmacies",
                column: "ContactEmail",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_pharmacies_ContactPhone",
                table: "pharmacies",
                column: "ContactPhone",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_pharmacies_UserId",
                table: "pharmacies",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_doctor_availability_slots_doctors_DoctorId",
                table: "doctor_availability_slots",
                column: "DoctorId",
                principalTable: "doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_doctor_availability_slots_doctors_DoctorId",
                table: "doctor_availability_slots");

            migrationBuilder.DropTable(
                name: "pharmacies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_doctor_availability_slots",
                table: "doctor_availability_slots");

            migrationBuilder.RenameTable(
                name: "doctor_availability_slots",
                newName: "DoctorAvailabilitySlots");

            migrationBuilder.RenameIndex(
                name: "IX_doctor_availability_slots_DoctorId",
                table: "DoctorAvailabilitySlots",
                newName: "IX_DoctorAvailabilitySlots_DoctorId");

            migrationBuilder.AlterColumn<int>(
                name: "AppointmentType",
                table: "DoctorAvailabilitySlots",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(16)",
                oldMaxLength: 16);

            migrationBuilder.AddPrimaryKey(
                name: "PK_DoctorAvailabilitySlots",
                table: "DoctorAvailabilitySlots",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorAvailabilitySlots_doctors_DoctorId",
                table: "DoctorAvailabilitySlots",
                column: "DoctorId",
                principalTable: "doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
