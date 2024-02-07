using System.Security.Cryptography;

public static class Hasher
{
    public static string HashMD5(string? str)
    {
        if (str == null)
        {
            throw new NullReferenceException();
        }
        var inputBytes = System.Text.Encoding.UTF8.GetBytes(str);
        using var md5 = MD5.Create();
        return Convert.ToHexString(md5.ComputeHash(inputBytes));
    } 

    public static string DecodeMD5(string? str)
    {
        if (str == null)
        {
            throw new NullReferenceException();
        }
        var inputBytes = System.Text.Encoding.UTF8.GetBytes(str);
        using var md5 = MD5.Create();
        return Convert.ToHexString(md5.ComputeHash(inputBytes));
    }
}