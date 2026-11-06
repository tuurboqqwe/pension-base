using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using PensionBase.Models;

namespace PensionBase.Data
{
    public static class PensionerRepository
    {
        public static List<PensionType> GetPensionTypes()
        {
            var result = new List<PensionType>();
            using (var connection = Db.Open())
            using (var command = new SqlCommand("SELECT Id, Name FROM dbo.PensionTypes ORDER BY Id", connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                    result.Add(new PensionType { Id = (int)reader["Id"], Name = (string)reader["Name"] });
            }
            return result;
        }

        public static List<Pensioner> Search(string text, int? pensionTypeId)
        {
            const string sql = @"
                SELECT p.*, t.Name AS PensionTypeName
                FROM dbo.Pensioners p
                JOIN dbo.PensionTypes t ON t.Id = p.PensionTypeId
                WHERE (@text = N'' OR p.LastName + N' ' + p.FirstName + N' ' + ISNULL(p.MiddleName, N'') LIKE @pattern OR p.Snils LIKE @pattern)
                  AND (@typeId IS NULL OR p.PensionTypeId = @typeId)
                ORDER BY p.LastName, p.FirstName, p.MiddleName";

            text = (text ?? string.Empty).Trim();
            var result = new List<Pensioner>();

            using (var connection = Db.Open())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@text", text);
                command.Parameters.AddWithValue("@pattern", "%" + EscapeLike(text) + "%");
                command.Parameters.Add("@typeId", SqlDbType.Int).Value = (object)pensionTypeId ?? DBNull.Value;

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        result.Add(Read(reader));
                }
            }
            return result;
        }

        // Сложный запрос 1: сводка по видам пенсий с долей каждого вида от общего числа записей
        public static DataTable GetStatisticsByType()
        {
            const string sql = @"
                SELECT
                    t.Name AS [Вид пенсии],
                    COUNT(p.Id) AS [Записей],
                    CAST(100.0 * COUNT(p.Id) / NULLIF(SUM(COUNT(p.Id)) OVER (), 0) AS DECIMAL(5,1)) AS [Доля, %],
                    CAST(AVG(p.PensionAmount) AS DECIMAL(10,2)) AS [Средняя, руб.],
                    MIN(p.PensionAmount) AS [Минимум, руб.],
                    MAX(p.PensionAmount) AS [Максимум, руб.],
                    ISNULL(SUM(p.PensionAmount), 0) AS [Сумма в месяц, руб.]
                FROM dbo.PensionTypes t
                LEFT JOIN dbo.Pensioners p ON p.PensionTypeId = t.Id
                GROUP BY t.Id, t.Name
                ORDER BY [Записей] DESC, t.Name";

            return Fill(sql);
        }

        // Сложный запрос 2: кто получает меньше среднего по своему виду пенсии и на сколько
        public static DataTable GetBelowAverage()
        {
            const string sql = @"
                WITH Ranked AS
                (
                    SELECT
                        p.LastName + N' ' + p.FirstName + ISNULL(N' ' + p.MiddleName, N'') AS FullName,
                        t.Name AS PensionType,
                        p.PensionAmount,
                        AVG(p.PensionAmount) OVER (PARTITION BY p.PensionTypeId) AS TypeAverage,
                        RANK() OVER (PARTITION BY p.PensionTypeId ORDER BY p.PensionAmount) AS TypeRank
                    FROM dbo.Pensioners p
                    JOIN dbo.PensionTypes t ON t.Id = p.PensionTypeId
                )
                SELECT
                    FullName AS [ФИО],
                    PensionType AS [Вид пенсии],
                    PensionAmount AS [Размер, руб.],
                    CAST(TypeAverage AS DECIMAL(10,2)) AS [Среднее по виду, руб.],
                    CAST(TypeAverage - PensionAmount AS DECIMAL(10,2)) AS [Ниже среднего на, руб.],
                    TypeRank AS [Место в виде]
                FROM Ranked
                WHERE PensionAmount < TypeAverage
                ORDER BY [Ниже среднего на, руб.] DESC";

            return Fill(sql);
        }

        private static DataTable Fill(string sql)
        {
            var table = new DataTable();
            using (var connection = Db.Open())
            using (var adapter = new SqlDataAdapter(sql, connection))
                adapter.Fill(table);
            return table;
        }

        public static bool SnilsExists(string snils, int exceptId)
        {
            using (var connection = Db.Open())
            using (var command = new SqlCommand(
                "SELECT COUNT(*) FROM dbo.Pensioners WHERE Snils = @snils AND Id <> @id", connection))
            {
                command.Parameters.AddWithValue("@snils", snils);
                command.Parameters.AddWithValue("@id", exceptId);
                return (int)command.ExecuteScalar() > 0;
            }
        }

        public static void Insert(Pensioner pensioner)
        {
            const string sql = @"
                INSERT INTO dbo.Pensioners
                    (LastName, FirstName, MiddleName, BirthDate, Gender, Snils, Address, Phone, PensionTypeId, PensionAmount, AssignedDate)
                VALUES
                    (@lastName, @firstName, @middleName, @birthDate, @gender, @snils, @address, @phone, @typeId, @amount, @assignedDate)";

            Execute(sql, pensioner);
        }

        public static void Update(Pensioner pensioner)
        {
            const string sql = @"
                UPDATE dbo.Pensioners SET
                    LastName = @lastName, FirstName = @firstName, MiddleName = @middleName,
                    BirthDate = @birthDate, Gender = @gender, Snils = @snils,
                    Address = @address, Phone = @phone, PensionTypeId = @typeId,
                    PensionAmount = @amount, AssignedDate = @assignedDate
                WHERE Id = @id";

            Execute(sql, pensioner);
        }

        public static void Delete(int id)
        {
            using (var connection = Db.Open())
            using (var command = new SqlCommand("DELETE FROM dbo.Pensioners WHERE Id = @id", connection))
            {
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
            }
        }

        private static void Execute(string sql, Pensioner p)
        {
            using (var connection = Db.Open())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", p.Id);
                command.Parameters.AddWithValue("@lastName", p.LastName);
                command.Parameters.AddWithValue("@firstName", p.FirstName);
                command.Parameters.AddWithValue("@middleName", NullIfEmpty(p.MiddleName));
                command.Parameters.Add("@birthDate", SqlDbType.Date).Value = p.BirthDate;
                command.Parameters.AddWithValue("@gender", p.Gender);
                command.Parameters.AddWithValue("@snils", p.Snils);
                command.Parameters.AddWithValue("@address", NullIfEmpty(p.Address));
                command.Parameters.AddWithValue("@phone", NullIfEmpty(p.Phone));
                command.Parameters.AddWithValue("@typeId", p.PensionTypeId);
                command.Parameters.Add("@amount", SqlDbType.Decimal).Value = p.PensionAmount;
                command.Parameters["@amount"].Precision = 10;
                command.Parameters["@amount"].Scale = 2;
                command.Parameters.Add("@assignedDate", SqlDbType.Date).Value = p.AssignedDate;
                command.ExecuteNonQuery();
            }
        }

        private static Pensioner Read(SqlDataReader reader)
        {
            return new Pensioner
            {
                Id = (int)reader["Id"],
                LastName = (string)reader["LastName"],
                FirstName = (string)reader["FirstName"],
                MiddleName = reader["MiddleName"] as string,
                BirthDate = (DateTime)reader["BirthDate"],
                Gender = (string)reader["Gender"],
                Snils = (string)reader["Snils"],
                Address = reader["Address"] as string,
                Phone = reader["Phone"] as string,
                PensionTypeId = (int)reader["PensionTypeId"],
                PensionTypeName = (string)reader["PensionTypeName"],
                PensionAmount = (decimal)reader["PensionAmount"],
                AssignedDate = (DateTime)reader["AssignedDate"]
            };
        }

        private static object NullIfEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value.Trim();
        }

        private static string EscapeLike(string value)
        {
            return value.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
        }
    }
}
