using System;
using System.Security.Cryptography;
using System.Text;

namespace PddTrainingApp.Services
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16; // 128 бит
        private const int HashSize = 32; // 256 бит
        private const int Iterations = 100000; // Количество итераций PBKDF2

        public static string HashPassword(string password)
        {
            // Генерируем случайную соль
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] salt = new byte[SaltSize];
                rng.GetBytes(salt);

                // Создаем хэш с помощью PBKDF2
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                {
                    byte[] hash = pbkdf2.GetBytes(HashSize);

                    // Комбинируем соль и хэш
                    byte[] hashBytes = new byte[SaltSize + HashSize];
                    Array.Copy(salt, 0, hashBytes, 0, SaltSize);
                    Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

                    return Convert.ToBase64String(hashBytes);
                }
            }
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            try
            {
                // Извлекаем байты из хранимого хэша
                byte[] hashBytes = Convert.FromBase64String(storedHash);

                // Извлекаем соль (первые SaltSize байт)
                byte[] salt = new byte[SaltSize];
                Array.Copy(hashBytes, 0, salt, 0, SaltSize);

                // Создаем хэш введенного пароля с той же солью
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                {
                    byte[] hash = pbkdf2.GetBytes(HashSize);

                    // Сравниваем хэши
                    for (int i = 0; i < HashSize; i++)
                    {
                        if (hashBytes[i + SaltSize] != hash[i])
                            return false;
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}