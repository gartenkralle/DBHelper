using System.Data.SqlClient;

namespace DBHelper
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine($"Call this program as follows: " +
                            $"{AppDomain.CurrentDomain.FriendlyName}.exe server database getTables|getColumns column|table");

                return;
            }

            List<string> lines = ReadTableData(server: args[0], database: args[1]);

            if (args[2] == "getTables")
            {
                PrintResult(GetTableDictionary(lines), key: args[3], "Tables");
            }
            else if (args[2] == "getColumns")
            {
                PrintResult(GetColumnDictionary(lines), key: args[3], "Columns");
            }
        }

        private static void PrintResult(Dictionary<string, List<string>> dict, string key, string header)
        {
            string line = key.Trim().ToUpper();

            if (dict.TryGetValue(line, out List<string>? values))
            {
                Console.WriteLine();
                Console.WriteLine(header);
                Console.WriteLine($"-------------------------------------");
                Console.WriteLine(string.Join(Environment.NewLine, values));
            }
        }

        private static Dictionary<string, List<string>> GetTableDictionary(List<string> lines)
        {
            Dictionary<string, List<string>> result = new();

            foreach (string line in lines)
            {
                string[] row = line.Split(',');

                string table = row[0];
                string column = row[1].Replace("\"", "").Trim().ToUpper();


                if (!result.ContainsKey(column))
                {
                    result.Add(key: column, new List<string>() { table });
                }
                else
                {
                    result[key: column].Add(table);
                }
            }

            return result;
        }

        private static Dictionary<string, List<string>> GetColumnDictionary(List<string> lines)
        {
            Dictionary<string, List<string>> result = new();

            foreach (string line in lines)
            {
                string[] row = line.Split(',');

                string table = row[0].Replace("\"", "").Trim().ToUpper();
                string column = row[1].Trim();

                if (!result.ContainsKey(table))
                {
                    result.Add(key: table, new List<string>() { column });
                }
                else
                {
                    result[key: table].Add(column);
                }
            }

            return result;
        }

        private static List<string> ReadTableData(string server, string database)
        {
            List<string> result = new();

            string queryString =
                "select tab.name as table_name, col.name as column_name " +
                "from sys.tables as tab inner join sys.columns as col on tab.object_id = col.object_id " +
                "where schema_name(tab.schema_id)='dbo' " +
                "order by table_name, column_id;";

            using (SqlConnection sqlConnection = new($"Data Source={server};Initial Catalog={database};Integrated Security=True"))
            {
                SqlCommand sqlCommand = new(queryString, sqlConnection);
                sqlConnection.Open();

                using SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();

                while (sqlDataReader.Read())
                {
                    result.Add($"{sqlDataReader[0]}, {sqlDataReader[1]}");
                }
            }

            return result;
        }
    }
}
