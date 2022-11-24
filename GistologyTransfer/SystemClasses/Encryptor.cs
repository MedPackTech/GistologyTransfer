using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GistologyTransfer
{
    /// <summary>
    /// Универсальный энкриптор. В приложении шифрует строку подключения к БД.
    /// </summary>
    public class Encryptor
    {
        private static readonly string saltKey = "S#rTKBIf"; // Длина 8 знаков
        private static readonly string IVKey = "Df$H9jep&YUQwsz4"; // Длина 16 знаков
        private static readonly string word = "45DA-AB14-0BBB66ED6C65";

        /// <summary>
        /// Шифрует переданную строку AES.
        /// </summary>
        /// <param name="plainText">Строка подлежащая шифрованию.</param>
        /// <returns>Зашифрованная строка.</returns>
        public static string AES_Ecnrypt(string plainText)
        {
            if (string.IsNullOrWhiteSpace(plainText))
            {
                return string.Empty;
            }

            byte[] encryptedBytes = null;
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] saltKeyBytes = Encoding.ASCII.GetBytes(saltKey);
            byte[] IVKeyBytes = Encoding.ASCII.GetBytes(IVKey);

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(word, saltKeyBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    //AES.IV = key.GetBytes(AES.BlockSize / 8);
                    AES.IV = IVKeyBytes;
                    AES.Mode = CipherMode.CBC;

                    using (CryptoStream cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(plainTextBytes, 0, plainTextBytes.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }
            return Convert.ToBase64String(encryptedBytes);
        }

        /// <summary>
        /// Шифрует переданную строку AES. Асинхронная реализация.
        /// </summary>
        /// <param name="plainText">Строка подлежащая шифрованию.</param>
        /// <returns>Зашифрованная строка.</returns>
        public static Task<string> AES_EcnryptAsync(string plainText)
        {
            return Task.Factory.StartNew(() => { return AES_Ecnrypt(plainText); });
        }

        /// <summary>
        /// Дешифрует переданную строку AES.
        /// </summary>
        /// <param name="encryptedText">Строка подлежащая дешифрованию.</param>
        /// <returns></returns>
        public static string AES_Decrypt(string encryptedText)
        {
            if (string.IsNullOrWhiteSpace(encryptedText))
            {
                return string.Empty;
            }

            byte[] decryptedBytes = null;
            byte[] encryptedTextBytes = Convert.FromBase64String(encryptedText);
            byte[] saltKeyBytes = Encoding.ASCII.GetBytes(saltKey);
            byte[] IVKeyBytes = Encoding.ASCII.GetBytes(IVKey);

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(word, saltKeyBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    //AES.IV = key.GetBytes(AES.BlockSize / 8);
                    AES.IV = IVKeyBytes;
                    AES.Mode = CipherMode.CBC;

                    using (CryptoStream cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(encryptedTextBytes, 0, encryptedTextBytes.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();
                }
            }
            //return Encoding.UTF8.GetString(decryptedBytes).TrimEnd("\0".ToCharArray());
            return Encoding.UTF8.GetString(decryptedBytes);
        }

        /// <summary>
        /// Дешифрует переданную строку AES. Асинхронная реализация.
        /// </summary>
        /// <param name="encryptedText">Строка подлежащая дешифрованию.</param>
        /// <returns></returns>
        public static Task<string> AES_DecryptAsync(string encryptedText)
        {
            return Task.Factory.StartNew(() => { return AES_Decrypt(encryptedText); });
        }
    }
}
