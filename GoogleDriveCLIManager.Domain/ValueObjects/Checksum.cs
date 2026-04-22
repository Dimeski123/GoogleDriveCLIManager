using System.Text.Json.Serialization;

namespace GoogleDriveCLIManager.Domain.ValueObjects;

public record Checksum
{
    [JsonConstructor]
    private Checksum(string value)
    {
        Value = value;
    }

    public string Value { get; init; }

    public static Checksum Create(string? md5Hash)
    {
        if (string.IsNullOrWhiteSpace(md5Hash))
            throw new ArgumentException("Checksum cannot be null or empty.");

        if (md5Hash.Length != 32)
            throw new ArgumentException($"Invalid MD5 format. Expected 32 chars, got {md5Hash.Length}.");

        return new Checksum(md5Hash.ToLowerInvariant());
    }

    public override string ToString() => Value;
}
