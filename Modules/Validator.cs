using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.ComponentModel.DataAnnotations;

namespace Common
{
    public static class Validator
    {
		private const int MIN_LENGTH = 6;
        private const int MAX_LENGTH = 20;
        private static  EmailAddressAttribute foo = new EmailAddressAttribute();
		public static Random random = new Random();
        private static string Alphavite = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        private static string sum_names = "abc123";

        public static bool ValidateEmail(string email)
        {
            bool bar = foo.IsValid(email);
            Log.Info("Validating email=" + email + " success=" + bar);
            return bar;
        } 
		public static bool ValidatePassword(string password, ref string answer) 
		{
			bool hasLetter = false;
            bool hasDecimalDigit = false;
            bool meetsLengthRequirements = false;
            if (!string.IsNullOrEmpty(password)) 
            {
                meetsLengthRequirements = password.Length >= MIN_LENGTH && password.Length <= MAX_LENGTH;
                if (meetsLengthRequirements)
                {
                    foreach (char c in password)
                    {
                        if (char.IsLetter(c)) hasLetter = true;
                        else if (char.IsDigit(c)) hasDecimalDigit = true;
                    }
                    if (hasLetter)
                    {
                        if (hasDecimalDigit)
                        {

                        }
                        else
                        {
                            answer = "Current password doesn't has decimal digit.";
                        }
                    }
                    else
                    {
                        answer = "Current password doesn't has letter.";
                    }
                }
                else
                {
                    answer = "Password must be more than 6 characters and less that 20.";
                }
            }
            else
            {
                answer = "Password must be more than 6 characters.";
            }
            bool isValid = meetsLengthRequirements && hasDecimalDigit && hasLetter;
            Common.Log.Info("Validate password success=" + isValid + ".");
			return isValid;         
        }
        public static string GenerateHash(int length_hash)
        {
            string hash = "";
            for (int i = 0; i < length_hash; i++)
            {
                hash += Alphavite[random.Next(Alphavite.Length)];
            }
            return hash;
        }
        public static string HashPassword(string password)
        {
            byte[] salt;
            byte[] buffer2;
            if (password == null)
            {
                Common.Log.Error("Input value is null, function HashPassword()");
                return "";
            }
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, 0x10, 0x3e8))
            {
                salt = bytes.Salt;
                buffer2 = bytes.GetBytes(0x20);
            }
            byte[] dst = new byte[0x31];
            Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
            Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
            return Convert.ToBase64String(dst);
        }
        public static bool VerifyHashedPassword(string hashedPassword, string password)
        {
            byte[] buffer4;
            if (hashedPassword == null)
            {
                return false;
            }
            if (password == null)
            {
                return false;
            }
            byte[] src = Convert.FromBase64String(hashedPassword);
            if ((src.Length != 0x31) || (src[0] != 0))
            {
                return false;
            }
            byte[] dst = new byte[0x10];
            Buffer.BlockCopy(src, 1, dst, 0, 0x10);
            byte[] buffer3 = new byte[0x20];
            Buffer.BlockCopy(src, 0x11, buffer3, 0, 0x20);
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, dst, 0x3e8))
            {
                buffer4 = bytes.GetBytes(0x20);
            }
            return ByteArraysEqual(ref buffer3,ref buffer4);
        }
        private static bool ByteArraysEqual(ref byte[] b1,ref byte[] b2)
        {
            if (b1 == b2)
            {
                return true;
            }
            if (b1 == null || b2 == null)
            { 
                return false; 
            }
            if (b1.Length != b2.Length)
            {
                return false;
            }
            for (int i = 0; i < b1.Length; i++)
            {
                if (b1[i] != b2[i]) return false;
            }
            return true;
        }
        public static string Encrypt(ref string clearText)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(sum_names,  new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }
        public static string Decrypt(ref string cipherText)
        {
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(sum_names, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
    }
}
