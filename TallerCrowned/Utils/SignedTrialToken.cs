// Utils/SignedTrialToken.cs
using System.Security.Cryptography;
using System.Text;

public static class SignedTrialToken
{
    // Devuelve un token compacto: base64url(email|expUnix|nonce|sig)
    public static string Create(string email, DateTime expiresAtUtc, string secret)
    {
        var emailNorm = email.Trim().ToLowerInvariant();
        var exp = new DateTimeOffset(expiresAtUtc).ToUnixTimeSeconds().ToString();
        var nonce = Guid.NewGuid().ToString("N"); // evita tokens idénticos

        var data = $"{emailNorm}|{exp}|{nonce}";
        var sig = Sign(data, secret);

        var packed = $"{data}|{sig}";
        return Base64UrlEncode(Encoding.UTF8.GetBytes(packed));
    }

    public static bool TryValidate(string token, string secret, out string email, out DateTime expUtc)
    {
        email = "";
        expUtc = DateTime.MinValue;

        string packed;
        try { packed = Encoding.UTF8.GetString(Base64UrlDecode(token)); }
        catch { return false; }

        var parts = packed.Split('|');
        if (parts.Length != 4) return false;

        email = parts[0];
        if (!long.TryParse(parts[1], out var expUnix)) return false;
        var nonce = parts[2];
        var sig = parts[3];

        var data = $"{email}|{expUnix}|{nonce}";
        var expected = Sign(data, secret);
        if (!CryptographicEquals(sig, expected)) return false;

        expUtc = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
        if (expUtc < DateTime.UtcNow) return false;

        return true;
    }

    private static string Sign(string data, string secret)
    {
        using var h = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var bytes = h.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Base64UrlEncode(bytes);
    }

    private static bool CryptographicEquals(string a, string b)
    {
        var ba = Encoding.UTF8.GetBytes(a);
        var bb = Encoding.UTF8.GetBytes(b);
        if (ba.Length != bb.Length) return false;
        var diff = 0;
        for (int i = 0; i < ba.Length; i++) diff |= ba[i] ^ bb[i];
        return diff == 0;
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] Base64UrlDecode(string s)
    {
        s = s.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4) { case 2: s += "=="; break; case 3: s += "="; break; }
        return Convert.FromBase64String(s);
    }
}
