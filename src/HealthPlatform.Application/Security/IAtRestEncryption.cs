namespace HealthPlatform.Application.Security;

/// <summary>
/// Application-level contract for AES-256 field/blob encryption before persistence (keys from configuration/KMS).
/// </summary>
public interface IAtRestEncryption
{
    /// <summary>
    /// Encrypts plaintext bytes and returns a base64 payload suitable for DB storage (includes nonce/tag as implemented).
    /// </summary>
    string Encrypt(ReadOnlySpan<byte> plaintext, CancellationToken ct = default);

    /// <summary>
    /// Decrypts a payload previously produced by <see cref="Encrypt"/>.
    /// </summary>
    byte[] Decrypt(ReadOnlySpan<char> ciphertextBase64, CancellationToken ct = default);
}
