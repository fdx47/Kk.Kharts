namespace Kk.Kharts.Api.Services.IService;

/// <summary>
/// Service for encoding and decoding integer IDs to obfuscated string hashes.
/// Uses HashIds algorithm to generate short, unique, non-sequential public IDs.
/// </summary>
public interface IHashIdService
{
    /// <summary>
    /// Encodes an integer ID to a hash string.
    /// </summary>
    /// <param name="id">The integer ID to encode.</param>
    /// <returns>The encoded hash string, or null if encoding fails.</returns>
    string? Encode(int id);

    /// <summary>
    /// Decodes a hash string back to the original integer ID.
    /// </summary>
    /// <param name="hash">The hash string to decode.</param>
    /// <returns>The decoded integer ID, or null if decoding fails or hash is invalid.</returns>
    int? Decode(string hash);

    /// <summary>
    /// Encodes a long ID to a hash string.
    /// </summary>
    /// <param name="id">The long ID to encode.</param>
    /// <returns>The encoded hash string, or null if encoding fails.</returns>
    string? EncodeLong(long id);

    /// <summary>
    /// Decodes a hash string back to the original long ID.
    /// </summary>
    /// <param name="hash">The hash string to decode.</param>
    /// <returns>The decoded long ID, or null if decoding fails or hash is invalid.</returns>
    long? DecodeLong(string hash);
}
