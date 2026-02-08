using HashidsNet;
using Kk.Kharts.Api.Services.IService;

namespace Kk.Kharts.Api.Services;

/// <summary>
/// Implementation of IHashIdService using HashidsNet library.
/// Generates short, unique, non-sequential hash strings from integer IDs.
/// </summary>
public class HashIdService : IHashIdService
{
    private readonly Hashids _hashids;
    private const string Salt = "$KropKontrol$2025!!!2026!!!";
    private const int MinHashLength = 12;

    public HashIdService()
    {
        _hashids = new Hashids(Salt, MinHashLength);
    }

    /// <inheritdoc />
    public string? Encode(int id)
    {
        if (id <= 0)
            return null;

        try
        {
            return _hashids.Encode(id);
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc />
    public int? Decode(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            return null;

        try
        {
            var numbers = _hashids.Decode(hash);
            if (numbers.Length == 0)
                return null;

            return (int)numbers[0];
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc />
    public string? EncodeLong(long id)
    {
        if (id <= 0)
            return null;

        try
        {
            return _hashids.EncodeLong(id);
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc />
    public long? DecodeLong(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            return null;

        try
        {
            var numbers = _hashids.DecodeLong(hash);
            if (numbers.Length == 0)
                return null;

            return numbers[0];
        }
        catch
        {
            return null;
        }
    }
}
