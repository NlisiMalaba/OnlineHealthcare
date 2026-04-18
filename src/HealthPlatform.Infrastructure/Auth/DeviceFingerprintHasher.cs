using System.Security.Cryptography;
using System.Text;

namespace HealthPlatform.Infrastructure.Auth;

public static class DeviceFingerprintHasher
{
    public static string Hash(string clientMaterial)
    {
        var normalized = clientMaterial.Trim();
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(bytes);
    }
}
