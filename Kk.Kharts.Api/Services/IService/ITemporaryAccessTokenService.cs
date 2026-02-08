using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Services.IService;

public record TemporaryAccessValidationResult(bool Success, string? FailureReason, TemporaryAccessToken? Token = null);

public interface ITemporaryAccessTokenService
{
    /// <summary>
    /// Generates a new temporary access credential and persists it hashed.
    /// </summary>
    Task<(TemporaryAccessToken Token, string Plaintext)> GenerateAsync(
        User issuer,
        TimeSpan? lifetime = null,
        CancellationToken ct = default);

    /// <summary>
    /// Attempts to validate and consume a candidate temporary credential for the provided user.
    /// </summary>
    Task<TemporaryAccessValidationResult> TryConsumeAsync(
        string candidate,
        User targetUser,
        CancellationToken ct = default);
}
