//  //使用示例
//  class Program
// {
//     static void Main()
//     {
//         string key = "MySecretAESKey123";  // 用户自定义密钥
//         string originalText = "Hello, AES 加密!";

//         // 加密
//         string encryptedText = Utils.AESEncrypt(originalText, key);
//         Console.WriteLine("加密后：" + encryptedText);

//         // 解密
//         string decryptedText = Utils.AESDecrypt(encryptedText, key);
//         Console.WriteLine("解密后：" + decryptedText);
//     }
// }

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class Utils
{
    // AES加密方法
    public static string AESEncrypt(string plainText, string key)
    {
        try
        {
            byte[] keyBytes = GetKeyBytes(key);
            byte[] ivBytes = new byte[16]; // IV 固定为 16 字节的 0
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = keyBytes;
                aesAlg.IV = ivBytes;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("AES加密失败：" + ex.Message);
            return null;
        }
    }

    // AES解密方法
    public static string AESDecrypt(string cipherText, string key)
    {
        try
        {
            byte[] keyBytes = GetKeyBytes(key);
            byte[] ivBytes = new byte[16]; // IV 固定为 16 字节的 0
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = keyBytes;
                aesAlg.IV = ivBytes;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                {
                    return srDecrypt.ReadToEnd();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("AES解密失败：" + ex.Message);
            return null;
        }
    }

    // 处理密钥（确保 32 字节）
    private static byte[] GetKeyBytes(string key)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
        }
    }
}