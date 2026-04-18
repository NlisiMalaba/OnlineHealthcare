using System.Security.Cryptography;
using HealthPlatform.Application.Security;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthPlatform.Infrastructure.Security;

public sealed class Aes256AtRestEncryptionOptions
{
    public const string SectionName = "Security:AtRestEncryption";

    /// <summary>
    /// 32-byte key encoded as base64 (AES-256).
    /// </summary>
    public string? KeyBase64 { get; set; }
}

/// <summary>
/// AES-256-GCM at-rest encryption helper (random nonce per operation, authentication tag).
/// </summary>
public sealed class Aes256AtRestEncryption : IAtRestEncryption, IDisposable
{
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private readonly byte[] _key;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<Aes256AtRestEncryption> _logger;
    private readonly AesGcm _aes;

    public Aes256AtRestEncryption(
        IOptions<Aes256AtRestEncryptionOptions> options,
        IHostEnvironment environment,
        ILogger<Aes256AtRestEncryption> logger)
    {
        _environment = environment;
        _logger = logger;
        var keyBase64 = options.Value.KeyBase64;
        if (string.IsNullOrWhiteSpace(keyBase64))
        {
            if (environment.IsProduction())
            {
                throw new InvalidOperationException(
                    $"{Aes256AtRestEncryptionOptions.SectionName}:KeyBase64 must be configured in production.");
            }

            _logger.LogWarning(
                "At-rest encryption key is not configured; generating ephemeral dev-only key (not for production data).");
            _key = new byte[32];
            RandomNumberGenerator.Fill(_key);
        }
        else
        {
            _key = Convert.FromBase64String(keyBase64);
            if (_key.Length != 32)
            {
                throw new InvalidOperationException("At-rest encryption key must decode to exactly 32 bytes (AES-256).");
            }
        }

        _aes = new AesGcm(_key, TagSize);
    }

    public string Encrypt(ReadOnlySpan<byte> plaintext, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        Span<byte> nonce = stackalloc byte[NonceSize];
        RandomNumberGenerator.Fill(nonce);

        Span<byte> tag = stackalloc byte[TagSize];
        var ciphertext = new byte[plaintext.Length];
        _aes.Encrypt(nonce, plaintext, ciphertext, tag);

        // Layout: version(1) | nonce | tag | ciphertext
        var payload = new byte[1 + NonceSize + TagSize + ciphertext.Length];
        payload[0] = 1;
        nonce.CopyTo(payload.AsSpan(1, NonceSize));
        tag.CopyTo(payload.AsSpan(1 + NonceSize, TagSize));
        ciphertext.CopyTo(payload.AsSpan(1 + NonceSize + TagSize));

        return Convert.ToBase64String(payload);
    }

    public byte[] Decrypt(ReadOnlySpan<char> ciphertextBase64, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var payload = Convert.FromBase64String(new string(ciphertextBase64));
        if (payload.Length < 1 + NonceSize + TagSize)
        {
            throw new CryptographicException("Ciphertext payload is too short.");
        }

        if (payload[0] != 1)
        {
            throw new CryptographicException($"Unsupported ciphertext version: {payload[0]}.");
        }

        var nonce = payload.AsSpan(1, NonceSize);
        var tag = payload.AsSpan(1 + NonceSize, TagSize);
        var ciphertext = payload.AsSpan(1 + NonceSize + TagSize);
        var plaintext = new byte[ciphertext.Length];
        _aes.Decrypt(nonce, ciphertext, tag, plaintext);
        return plaintext;
    }

    public void Dispose() => _aes.Dispose();
}
