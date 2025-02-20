using System.Security.Cryptography;
using System.Text;

namespace StravaRunner.Helpers;

public static class EncryptionHelper
{
    public static string Encrypt(string plainText, string base64Key)
    {
        var key = Convert.FromBase64String(base64Key);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs, Encoding.UTF8))
        {
            sw.Write(plainText);
        }

        var iv = aes.IV;
        var encryptedBytes = ms.ToArray();

        var combined = new byte[iv.Length + encryptedBytes.Length];
        Buffer.BlockCopy(iv, 0, combined, 0, iv.Length);
        Buffer.BlockCopy(encryptedBytes, 0, combined, iv.Length, encryptedBytes.Length);

        return Convert.ToBase64String(combined);
    }

    public static string Decrypt(string cipherText, string base64Key)
    {
        var key = Convert.FromBase64String(base64Key);

        var combined = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = key;

        var iv = new byte[aes.BlockSize / 8];
        var encryptedBytes = new byte[combined.Length - iv.Length];

        Buffer.BlockCopy(combined, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(combined, iv.Length, encryptedBytes, 0, encryptedBytes.Length);

        aes.IV = iv;

        // Decrypt
        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(encryptedBytes);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs, Encoding.UTF8);

        var decrypted = sr.ReadToEnd();
        return decrypted;
    }
}
