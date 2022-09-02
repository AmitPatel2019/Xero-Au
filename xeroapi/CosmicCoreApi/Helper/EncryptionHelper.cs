using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace CosmicCoreApi.Helper
{
    public  class EncryptionHelper
    {
        public static string Encrpt(string plaintext)
        {
            UTF8Encoding utf8 = new UTF8Encoding();
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();

            string ciphertext;
            aes.Key = utf8.GetBytes("AMINHAKEYTEM32NYTES1234567891234");
            aes.IV = utf8.GetBytes("7061737323313233");

            using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            {
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
                byte[] bytes = utf8.GetBytes(plaintext);
                cs.Write(bytes, 0, bytes.Length);
                cs.FlushFinalBlock();
                ms.Position = 0;
                bytes = new byte[ms.Length];
                ms.Read(bytes, 0, bytes.Length);
                ciphertext = Convert.ToBase64String(bytes);
            }
            return ciphertext;
        }

        public static string Decrypt(string encrypted)
        {
            string decryptedtext;

            UTF8Encoding utf8 = new UTF8Encoding();
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();

            aes.Key = utf8.GetBytes("AMINHAKEYTEM32NYTES1234567891234");
            aes.IV = utf8.GetBytes("7061737323313233");


            using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            {
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write);
                byte[] bytes = Convert.FromBase64String(encrypted);
                cs.Write(bytes, 0, bytes.Length);
                cs.FlushFinalBlock();
                ms.Position = 0;
                bytes = new byte[ms.Length];
                ms.Read(bytes, 0, bytes.Length);
                decryptedtext = utf8.GetString(bytes);
            }
            return decryptedtext;
        }
    }
}