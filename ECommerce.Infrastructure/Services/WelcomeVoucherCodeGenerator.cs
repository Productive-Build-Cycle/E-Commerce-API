using System.Security.Cryptography;

namespace ECommerce.Infrastructure.Services;

public sealed class WelcomeVoucherCodeGenerator
{
    private const string Prefix = "WELCOME20-";
    private const string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const int SuffixLength = 6;

    public string Generate()
    {
        var suffix = new char[SuffixLength];

        for (var index = 0; index < suffix.Length; index++)
        {
            suffix[index] = Characters[
                RandomNumberGenerator.GetInt32(Characters.Length)];
        }

        return $"{Prefix}{new string(suffix)}".ToUpperInvariant();
    }
}
