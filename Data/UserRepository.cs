using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using PensionBase.Models;

namespace PensionBase.Data
{
    public static class UserRepository
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100000;

        public static bool LoginExists(string login)
        {
            using (var connection = Db.Open())
            using (var command = new SqlCommand("SELECT COUNT(*) FROM dbo.Users WHERE Login = @login", connection))
            {
                command.Parameters.AddWithValue("@login", login);
                return (int)command.ExecuteScalar() > 0;
            }
        }

        public static void Register(string login, string fullName, string password)
        {
            var salt = new byte[SaltSize];
            using (var random = RandomNumberGenerator.Create())
                random.GetBytes(salt);

            using (var connection = Db.Open())
            using (var command = new SqlCommand(
                "INSERT INTO dbo.Users (Login, FullName, PasswordHash, PasswordSalt) VALUES (@login, @fullName, @hash, @salt)",
                connection))
            {
                command.Parameters.AddWithValue("@login", login);
                command.Parameters.AddWithValue("@fullName", fullName);
                command.Parameters.Add("@hash", SqlDbType.VarBinary, HashSize).Value = Hash(password, salt);
                command.Parameters.Add("@salt", SqlDbType.VarBinary, SaltSize).Value = salt;
                command.ExecuteNonQuery();
            }
        }

        // Возвращает пользователя или null, если логин или пароль неверные
        public static User Authenticate(string login, string password)
        {
            using (var connection = Db.Open())
            using (var command = new SqlCommand(
                "SELECT Id, Login, FullName, PasswordHash, PasswordSalt FROM dbo.Users WHERE Login = @login",
                connection))
            {
                command.Parameters.AddWithValue("@login", login);
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    var expected = (byte[])reader["PasswordHash"];
                    var salt = (byte[])reader["PasswordSalt"];
                    if (!SameBytes(expected, Hash(password, salt)))
                        return null;

                    return new User
                    {
                        Id = (int)reader["Id"],
                        Login = (string)reader["Login"],
                        FullName = (string)reader["FullName"]
                    };
                }
            }
        }

        private static byte[] Hash(string password, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                return pbkdf2.GetBytes(HashSize);
        }

        private static bool SameBytes(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;

            var difference = 0;
            for (var i = 0; i < a.Length; i++)
                difference |= a[i] ^ b[i];
            return difference == 0;
        }
    }
}
