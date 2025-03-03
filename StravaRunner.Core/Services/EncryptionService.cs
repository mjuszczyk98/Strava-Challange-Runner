using Microsoft.Extensions.Options;
using StravaRunner.Core.Extensions;
using StravaRunner.Core.Models.Settings;

namespace StravaRunner.Core.Services;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}

public class EncryptionService(IOptions<SecuritySettings> securitySettings) : IEncryptionService
{
    private readonly SecuritySettings _securitySettings = securitySettings.Value;

    public string Encrypt(string plainText) => plainText.Encrypt(_securitySettings.EncryptTokenKey);

    public string Decrypt(string cipherText) => cipherText.Decrypt(_securitySettings.EncryptTokenKey);
}