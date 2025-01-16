using Fardin;
using Mahi.Settings;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using YamlDotNet.Core.Tokens;

namespace Mahi.LuaCore
{
    public class SessionAuthentication
    {
        private HttpRequest request;
        private HttpResponse response;

        public SessionAuthentication(HttpRequest request, HttpResponse response)
        {
            this.request = request;
            this.response = response;
        }

        public bool isAuth()
        {
            return request.Cookies.Any(i => i.Name == (AppConfig.Instance.Auth.Name ?? "Mahi_Auth_Key"));
        }

        public void set(string name, bool keep = false)
        {
            byte[] key = AppConfig.Instance.Auth.GetKeyBytes();

            byte[] encryptionKey = new byte[16];
            byte[] signingKey = new byte[16];
            Buffer.BlockCopy(key, 0, encryptionKey, 0, 16);
            Buffer.BlockCopy(key, 16, signingKey, 0, 16);

            byte[] bytes = EncryptAndSign(name, encryptionKey, signingKey);

            DateTime expireDate = DateTime.Now.AddMinutes(AppConfig.Instance.Auth.Timeout ?? 60); // read from config
            response.Cookies.AddCookie(new HttpCookie(AppConfig.Instance.Auth.Name ?? "Mahi_Auth_Key", Convert.ToBase64String(bytes),
                AppConfig.Instance.Auth.Path ?? "/", SameSiteMode.Strict, true, true, keep ? expireDate : null));
        }

        public string name
        {
            get
            {
                byte[] key = AppConfig.Instance.Auth.GetKeyBytes();

                byte[] encryptionKey = new byte[16];
                byte[] signingKey = new byte[16];
                Buffer.BlockCopy(key, 0, encryptionKey, 0, 16);
                Buffer.BlockCopy(key, 16, signingKey, 0, 16);

                byte[] bytes = Convert.FromBase64String(request.Cookies.FirstOrDefault(i => i.Name == (AppConfig.Instance.Auth.Name ?? "Mahi_Auth_Key"))?.Value);
                return DecryptAndVerify(bytes, encryptionKey, signingKey);
            }
        }

        public void clear()
        {
            response.Cookies.RemoveCookie(AppConfig.Instance.Auth.Name ?? "Mahi_Auth_Key");
        }
        public string group()
        {
            return "";
        }

        public bool isInGroup(string name)
        {
            return false;
        }

        static byte[] EncryptData(byte[] data, byte[] key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.GenerateIV(); // تولید یک IV تصادفی

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new System.IO.MemoryStream())
                {
                    // اضافه کردن IV به ابتدای داده‌های رمزنگاری‌شده
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    // رمزنگاری داده‌ها
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                    }

                    return ms.ToArray();
                }
            }
        }

        static byte[] SignData(byte[] data, byte[] key)
        {
            using (HMACSHA256 hmac = new HMACSHA256(key))
                return hmac.ComputeHash(data);
        }

        static byte[] EncryptAndSign(string data, byte[] encryptionKey, byte[] signingKey)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            // رمزنگاری داده‌ها
            byte[] encryptedData = EncryptData(dataBytes, encryptionKey);

            // امضای داده‌های رمزنگاری‌شده
            byte[] signature = SignData(encryptedData, signingKey);

            // ترکیب داده‌های رمزنگاری‌شده و امضا
            byte[] result = new byte[encryptedData.Length + signature.Length];
            Buffer.BlockCopy(encryptedData, 0, result, 0, encryptedData.Length);
            Buffer.BlockCopy(signature, 0, result, encryptedData.Length, signature.Length);

            return result;
        }

        static byte[] DecryptData(byte[] encryptedData, byte[] key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;

                // استخراج IV از ابتدای داده‌های رمزنگاری‌شده
                byte[] iv = new byte[16];
                Buffer.BlockCopy(encryptedData, 0, iv, 0, iv.Length);
                aes.IV = iv;

                // رمزگشایی داده‌ها (بدون شامل کردن IV)
                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var ms = new System.IO.MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(encryptedData, iv.Length, encryptedData.Length - iv.Length);
                    }

                    return ms.ToArray();
                }
            }
        }

        static string DecryptAndVerify(byte[] encryptedAndSignedData, byte[] encryptionKey, byte[] signingKey)
        {
            // طول امضا برای HMAC-SHA256 برابر با ۳۲ بایت است
            int signatureLength = 32;
            byte[] signature = new byte[signatureLength];
            Buffer.BlockCopy(encryptedAndSignedData, encryptedAndSignedData.Length - signatureLength, signature, 0, signatureLength);

            // استخراج داده‌های رمزنگاری‌شده (بدون امضا)
            byte[] encryptedData = new byte[encryptedAndSignedData.Length - signatureLength];
            Buffer.BlockCopy(encryptedAndSignedData, 0, encryptedData, 0, encryptedData.Length);

            // تأیید امضا
            byte[] computedSignature = SignData(encryptedData, signingKey);
            if (!computedSignature.SequenceEqual(signature))
            {
                throw new Exception("امضا نامعتبر است! داده‌ها ممکن است تغییر کرده باشند.");
            }

            // رمزگشایی داده‌ها
            byte[] decryptedData = DecryptData(encryptedData, encryptionKey);
            return Encoding.UTF8.GetString(decryptedData);
        }
    }
}
