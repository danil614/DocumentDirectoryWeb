using System.Security.Cryptography;
using System.Text;

namespace DocumentDirectoryWeb.Helpers;

public static class HashPassword
{
    public static string GetHash(string? input)
    {
        if (string.IsNullOrEmpty(input)) return "";

        var hash = MD5.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hash);
    }
}