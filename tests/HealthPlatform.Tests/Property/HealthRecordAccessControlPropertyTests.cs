using FsCheck.Xunit;
using HealthPlatform.Application.Audit;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.HealthRecords.GrantHealthRecordAccess;
using HealthPlatform.Application.HealthRecords.ListHealthRecordEntries;
using HealthPlatform.Application.HealthRecords.RevokeHealthRecordAccess;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Domain.Audit;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Arbitraries;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Tests.Properties;

public sealed class HealthRecordAccessControlPropertyTests
{
    private static readonly DateTime ReferenceNowUtc = new(2026, 7, 3, 10, 0, 0, DateTimeKind.Utc);

    // Feature: online-healthcare-platform, Property 24: Health Record Access Control
    [Property(Arbitrary = [typeof(HealthRecordAccessArbitraries)], MaxTest = 500)]
    public bool Doctor_read_succeeds_if_and_only_if_active_grant_exists(HealthRecordAccessCase input) =>
        RunAccessControlInvariantAsync(input).GetAwaiter().GetResult();

    private static async Task<bool> RunAccessControlInvariantAsync(HealthRecordAccessCase input)
    {
        var clock = new FakeTimeProvider(ReferenceNowUtc);
        await using var host = new PatientRegistrationTestHost(timeProvider: clock);

        var patient = await SeedPatientAsync(host);
        var doctor = await SeedVerifiedDoctorAsync(host);
        var healthRecord = await host.DbContext.HealthRecords
            .AsNoTracking()
            .SingleAsync(record => record.PatientId == patient.Id);

        await ApplyGrantStateAsync(host, patient, doctor, healthRecord.Id, input);

        var deniedBefore = await CountDeniedAccessLogsAsync(host, doctor.Id, healthRecord.Id);
        var shouldAllowRead = input.GrantState == HealthRecordAccessGrantState.Active;

        host.CurrentUser.UserId = doctor.UserId;

        try
        {
            await host.Sender.Send(new ListHealthRecordEntriesQuery(healthRecord.Id), CancellationToken.None);

            if (!shouldAllowRead)
            {
                return false;
            }
        }
        catch (AccessDeniedException ex)
        {
            if (shouldAllowRead || ex.Code != "ACCESS_DENIED")
            {
                return false;
            }

            var deniedAfter = await CountDeniedAccessLogsAsync(host, doctor.Id, healthRecord.Id);
            return deniedAfter == deniedBefore + 1;
        }

        if (shouldAllowRead)
        {
            var deniedAfter = await CountDeniedAccessLogsAsync(host, doctor.Id, healthRecord.Id);
            return deniedAfter == deniedBefore;
        }

        return false;
    }

    private static async Task ApplyGrantStateAsync(
        PatientRegistrationTestHost host,
        Patient patient,
        Doctor doctor,
        Guid healthRecordId,
        HealthRecordAccessCase input)
    {
        if (input.GrantState == HealthRecordAccessGrantState.None)
        {
            return;
        }

        host.CurrentUser.UserId = patient.UserId;
        await host.Sender.Send(
            new GrantHealthRecordAccessCommand(
                doctor.Id,
                input.AccessType,
                Sections: null),
            CancellationToken.None);

        if (input.GrantState == HealthRecordAccessGrantState.Revoked)
        {
            await host.Sender.Send(new RevokeHealthRecordAccessCommand(doctor.Id), CancellationToken.None);
        }

        var activeGrantExists = await host.DbContext.HealthRecordAccesses
            .AsNoTracking()
            .AnyAsync(access =>
                access.HealthRecordId == healthRecordId
                && access.GrantedToDoctorId == doctor.Id
                && access.RevokedAtUtc == null);

        var expectedActive = input.GrantState == HealthRecordAccessGrantState.Active;
        if (activeGrantExists != expectedActive)
        {
            throw new InvalidOperationException("Test setup failed to establish expected grant state.");
        }
    }

    private static Task<int> CountDeniedAccessLogsAsync(
        PatientRegistrationTestHost host,
        Guid doctorId,
        Guid healthRecordId) =>
        host.DbContext.AuditLogs
            .AsNoTracking()
            .CountAsync(log =>
                log.Action == AuditActions.HealthRecordAccessDenied
                && log.ActorId == doctorId
                && log.ResourceId == healthRecordId);

    private static async Task<Doctor> SeedVerifiedDoctorAsync(PatientRegistrationTestHost host)
    {
        var registration = await host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);
        await host.Sender.Send(new VerifyDoctorLicenseCommand(registration.DoctorId), CancellationToken.None);
        return await host.DbContext.Doctors.SingleAsync(d => d.Id == registration.DoctorId);
    }

    private static async Task<Patient> SeedPatientAsync(PatientRegistrationTestHost host)
    {
        await host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Property Health Record Patient",
                null,
                $"property-hr-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
    }
}
