using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Security.Cryptography;
using System.Text;
using Kk.Kharts.Api.Data;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.Metrics;

namespace Kk.Kharts.Api.Services;

public class TemporaryAccessTokenService : ITemporaryAccessTokenService
{
    private const int PrefixLength = 8;
    private static readonly TimeSpan DefaultLifetime = TimeSpan.FromMinutes(20);

    private readonly AppDbContext _db;
    private readonly ILogger<TemporaryAccessTokenService> _logger;
    private readonly Counter<long> _tokensIssuedCounter;
    private readonly Counter<long> _tokensConsumedCounter;
    private readonly Counter<long> _tokenFailuresCounter;
    private readonly Histogram<double> _tokenLifetimeHistogram;

    public TemporaryAccessTokenService(
        AppDbContext db,
        ILogger<TemporaryAccessTokenService> logger,
        IMeterFactory meterFactory)
    {
        _db = db;
        _logger = logger;

        var meter = meterFactory.Create("Kk.Kharts.TempTokens");
        _tokensIssuedCounter = meter.CreateCounter<long>(
            name: "kk.temporal_tokens.issued",
            unit: "tokens",
            description: "Nombre de mots de passe temporaires émis");
        _tokensConsumedCounter = meter.CreateCounter<long>(
            name: "kk.temporal_tokens.consumed",
            unit: "tokens",
            description: "Nombre de mots de passe temporaires consommés");
        _tokenFailuresCounter = meter.CreateCounter<long>(
            name: "kk.temporal_tokens.failures",
            unit: "attempts",
            description: "Tentatives de validation échouées pour les mots de passe temporaires");
        _tokenLifetimeHistogram = meter.CreateHistogram<double>(
            name: "kk.temporal_tokens.lifetime_minutes",
            unit: "minutes",
            description: "Durée de vie configurée pour les mots de passe temporaires");
    }

    public async Task<(TemporaryAccessToken Token, string Plaintext)> GenerateAsync(
        User issuer,
        TimeSpan? lifetime = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(issuer);

        var displayToken = GenerateDisplayToken();
        var canonical = Canonicalize(displayToken);
        var now = DateTime.UtcNow;
        var expiresAt = now.Add(lifetime ?? DefaultLifetime);

        var entity = new TemporaryAccessToken
        {
            TokenHash = ComputeHash(canonical),
            TokenPrefix = canonical[..PrefixLength],
            CreatedAtUtc = now,
            ExpiresAtUtc = expiresAt,
            IssuedByUserId = issuer.Id,
            IssuedByEmail = issuer.Email,
            IssuedByTelegramUserId = issuer.TelegramUserId,
            UsageCount = 0,
            Revoked = false
        };

        _db.TemporaryAccessTokens.Add(entity);
        await _db.SaveChangesAsync(ct);

        await CleanupExpiredTokensAsync(ct);

        var lifetimeMinutes = (entity.ExpiresAtUtc - entity.CreatedAtUtc).TotalMinutes;
        var issuerTags = CreateIssuerTags(issuer);
        _tokensIssuedCounter.Add(1, issuerTags);
        _tokenLifetimeHistogram.Record(lifetimeMinutes, issuerTags);

        _logger.LogInformation(
            "Temporary token issued {@TokenAudit}",
            new
            {
                entity.Id,
                IssuerEmail = issuer.Email,
                issuer.Role,
                LifetimeMinutes = Math.Round(lifetimeMinutes, 2),
                entity.ExpiresAtUtc
            });

        return (entity, displayToken);
    }

    public async Task<TemporaryAccessValidationResult> TryConsumeAsync(
        string candidate,
        User targetUser,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            _tokenFailuresCounter.Add(1, new KeyValuePair<string, object?>[] { new("failure_reason", "blank_or_whitespace") });
            return new TemporaryAccessValidationResult(false, "Code vide ou invalide");
        }

        var canonical = Canonicalize(candidate);
        if (canonical.Length < PrefixLength)
        {
            _tokenFailuresCounter.Add(1, new KeyValuePair<string, object?>[] { new("failure_reason", "short_code") });
            return new TemporaryAccessValidationResult(false, "Code invalide");
        }

        var prefix = canonical[..PrefixLength];
        var now = DateTime.UtcNow;
        var hash = ComputeHash(canonical);

        var candidates = await _db.TemporaryAccessTokens
            .Where(t => t.TokenPrefix == prefix && !t.Revoked && t.ExpiresAtUtc >= now)
            .OrderByDescending(t => t.CreatedAtUtc)
            .ToListAsync(ct);

        foreach (var token in candidates)
        {
            if (HashesMatch(token.TokenHash, hash))
            {
                token.UsageCount += 1;
                token.LastUsedAtUtc = now;
                token.ConsumedByUserId = targetUser.Id;
                token.ConsumedByEmail = targetUser.Email;
                token.Revoked = true;

                await _db.SaveChangesAsync(ct);

                _tokensConsumedCounter.Add(1, new KeyValuePair<string, object?>[]
                {
                    new("consumer_role", targetUser.Role ?? "unknown"),
                });

                _logger.LogInformation(
                    "Temporary token consumed {@TokenAudit}",
                    new
                    {
                        token.Id,
                        ConsumedBy = targetUser.Email,
                        targetUser.Role,
                        token.ConsumedByUserId,
                        token.LastUsedAtUtc
                    });

                return new TemporaryAccessValidationResult(true, null, token);
            }
        }

        _tokenFailuresCounter.Add(1, new KeyValuePair<string, object?>[] { new("failure_reason", "not_found_or_expired") });
        return new TemporaryAccessValidationResult(false, "Code expiré ou invalide");
    }

    private static string GenerateDisplayToken()
    {
        Span<byte> buffer = stackalloc byte[16];
        RandomNumberGenerator.Fill(buffer);
        var hex = Convert.ToHexString(buffer); // 32 chars

        return $"KK-{hex[..4]}-{hex[4..8]}-{hex[8..12]}-{hex[12..16]}-{hex[16..20]}-{hex[20..24]}-{hex[24..28]}-{hex[28..32]}";
    }

    private static string Canonicalize(string input)
    {
        var sb = new StringBuilder(input.Length);
        foreach (var ch in input)
        {
            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(char.ToUpperInvariant(ch));
            }
        }

        return sb.ToString();
    }

    private static string ComputeHash(string canonical)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
        return Convert.ToHexString(bytes);
    }

    private static bool HashesMatch(string storedHex, string candidateHex)
    {
        try
        {
            var storedBytes = Convert.FromHexString(storedHex);
            var candidateBytes = Convert.FromHexString(candidateHex);
            return CryptographicOperations.FixedTimeEquals(storedBytes, candidateBytes);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private async Task CleanupExpiredTokensAsync(CancellationToken ct)
    {
        var cutoff = DateTime.UtcNow.AddDays(-1);
        var expired = await _db.TemporaryAccessTokens
            .Where(t => t.ExpiresAtUtc < DateTime.UtcNow || (t.Revoked && t.LastUsedAtUtc < cutoff))
            .ToListAsync(ct);

        if (expired.Count == 0)
            return;

        _db.TemporaryAccessTokens.RemoveRange(expired);
        await _db.SaveChangesAsync(ct);
    }

    private static KeyValuePair<string, object?>[] CreateIssuerTags(User issuer)
        => new KeyValuePair<string, object?>[]
        {
            new("issuer_role", issuer.Role ?? "unknown"),
        };
}
