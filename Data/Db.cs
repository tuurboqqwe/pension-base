using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PensionBase.Data
{
    public static class Db
    {
        private static string ConnectionString =>
            ConfigurationManager.ConnectionStrings["PensionBase"].ConnectionString;

        public static SqlConnection Open()
        {
            var connection = new SqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        // Создаёт базу и таблицы при первом запуске. Скрипт вшит в программу как ресурс.
        public static void Initialize()
        {
            var master = new SqlConnectionStringBuilder(ConnectionString) { InitialCatalog = "master" };

            using (var connection = new SqlConnection(master.ConnectionString))
            {
                connection.Open();
                foreach (var batch in ReadBatches())
                {
                    if (string.IsNullOrWhiteSpace(batch))
                        continue;
                    using (var command = new SqlCommand(batch, connection))
                        command.ExecuteNonQuery();
                }
            }
        }

        private static string[] ReadBatches()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("PensionBase.Database.create_database.sql"))
            using (var reader = new StreamReader(stream))
            {
                // GO понимают только SSMS и sqlcmd, поэтому делим скрипт на части сами
                return Regex.Split(reader.ReadToEnd(), @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            }
        }
    }
}
