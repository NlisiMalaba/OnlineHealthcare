using System.Security.Cryptography;
using System.Text;

namespace HealthPlatform.Infrastructure.Telemedicine.Agora;

/// <summary>
/// Agora AccessToken2 (007) builder for RTC channel join.
/// </summary>
internal static class AgoraAccessTokenBuilder
{
    private const byte Version = 0x07;
    private const ushort RtcServiceType = 1;
    private const ushort PrivilegeJoinChannel = 1;
    private const ushort PrivilegePublishAudio = 2;
    private const ushort PrivilegePublishVideo = 3;
    private const ushort PrivilegePublishData = 4;

    public static string Build(
        string appId,
        string appCertificate,
        string channelName,
        uint uid,
        uint expireTimestamp)
    {
        Span<byte> saltBytes = stackalloc byte[4];
        RandomNumberGenerator.Fill(saltBytes);
        var salt = BitConverter.ToUInt32(saltBytes);
        var ts = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var message = new ServiceRtc(channelName, uid);
        message.AddPrivilege(PrivilegeJoinChannel, expireTimestamp);
        message.AddPrivilege(PrivilegePublishAudio, expireTimestamp);
        message.AddPrivilege(PrivilegePublishVideo, expireTimestamp);
        message.AddPrivilege(PrivilegePublishData, expireTimestamp);

        var signing = new AccessTokenContent(appId, ts, salt, [message]);
        return signing.Build(appCertificate);
    }

    private sealed class ServiceRtc(string channelName, uint uid)
    {
        private readonly Dictionary<ushort, uint> _privileges = [];

        public void AddPrivilege(ushort privilege, uint expireTimestamp) =>
            _privileges[privilege] = expireTimestamp;

        public byte[] Pack() =>
            PackString(channelName)
                .Concat(PackUInt32(uid))
                .Concat(PackMap(_privileges))
                .ToArray();

        private static byte[] PackMap(Dictionary<ushort, uint> map)
        {
            using var stream = new MemoryStream();
            stream.Write(PackUInt16((ushort)map.Count));
            foreach (var (key, value) in map)
            {
                stream.Write(PackUInt16(key));
                stream.Write(PackUInt32(value));
            }

            return stream.ToArray();
        }
    }

    private sealed class AccessTokenContent(string appId, uint ts, uint salt, IReadOnlyList<ServiceRtc> services)
    {
        public string Build(string appCertificate)
        {
            using var message = new MemoryStream();
            message.Write(PackString(appId));
            message.Write(PackUInt32(ts));
            message.Write(PackUInt32(salt));
            message.Write(PackUInt16((ushort)services.Count));

            foreach (var service in services)
            {
                message.Write(PackUInt16(RtcServiceType));
                message.Write(service.Pack());
            }

            var signature = ComputeHmac(appCertificate, message.ToArray());
            using var content = new MemoryStream();
            content.WriteByte(Version);
            content.Write(PackString(appId));
            content.Write(PackUInt32(ts));
            content.Write(PackUInt32(salt));
            content.Write(PackUInt16((ushort)services.Count));

            foreach (var service in services)
            {
                content.Write(PackUInt16(RtcServiceType));
                content.Write(service.Pack());
            }

            content.Write(signature);
            return Convert.ToBase64String(content.ToArray());
        }
    }

    private static byte[] ComputeHmac(string certificate, byte[] message)
    {
        var key = Encoding.UTF8.GetBytes(certificate);
        return HMACSHA256.HashData(key, message);
    }

    private static byte[] PackUInt16(ushort value)
    {
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        return bytes;
    }

    private static byte[] PackUInt32(uint value)
    {
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        return bytes;
    }

    private static byte[] PackString(string value)
    {
        var payload = Encoding.UTF8.GetBytes(value);
        return PackUInt16((ushort)payload.Length).Concat(payload).ToArray();
    }
}

internal static class StreamExtensions
{
    public static void Write(this MemoryStream stream, byte[] data) => stream.Write(data, 0, data.Length);
}
