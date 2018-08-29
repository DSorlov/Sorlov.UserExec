using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace Sorlov.Windows.UserExec
{

    public static class Crypto
    {
        public static string Crypt(string data, string password, bool encrypt)
        {
            var saltValues = new byte[] { 0x26, 0x19, 0x81, 0x4E, 0xA0, 0x6D, 0x95, 0x34, 0x26, 0x75, 0x64, 0x05, 0xF6 };

            var passwordDeriveBytes = new PasswordDeriveBytes(password, saltValues);

            #pragma warning disable 618
            var rijndael = Rijndael.Create();
            rijndael.Key = passwordDeriveBytes.GetBytes(32);
            rijndael.IV = passwordDeriveBytes.GetBytes(16);
            #pragma warning restore 618

            var iCryptoTransform = (encrypt) ? rijndael.CreateEncryptor() : rijndael.CreateDecryptor();

            var memoryStream = new MemoryStream();
            var cryptoStream = new CryptoStream(memoryStream, iCryptoTransform, CryptoStreamMode.Write);

            var byteData = encrypt ? Encoding.Unicode.GetBytes(data) : Convert.FromBase64String(data);

            try
            {
                cryptoStream.Write(byteData, 0, byteData.Length);
                cryptoStream.Close();
                return encrypt ? Convert.ToBase64String(memoryStream.ToArray()) : Encoding.Unicode.GetString(memoryStream.ToArray());
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}