namespace HealthPlatform.Application.Telemedicine;

public static class TelemedicineUid
{
    public static uint ForPatient(Guid patientId) => StableUid(patientId, 1);

    public static uint ForDoctor(Guid doctorId) => StableUid(doctorId, 2);

    private static uint StableUid(Guid id, byte roleSalt)
    {
        Span<byte> bytes = stackalloc byte[17];
        id.TryWriteBytes(bytes);
        bytes[16] = roleSalt;
        return BitConverter.ToUInt32(bytes) | 1u;
    }
}
