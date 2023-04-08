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

            List<string> lines = ReadTableData(args[0], args[1]);

            if (args[2] == "getTables")
            {
                PrintResult(GetTableDictionary(lines), args[3], "Tables");
            }
            else if (args[2] == "getColumns")
            {
                PrintResult(GetColumnDictionary(lines), args[3], "Columns");
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
                string[] cols = line.Split(',');

                cols[1] = cols[1].Replace("\"", "").Trim().ToUpper();

                if (!result.ContainsKey(cols[1]))
                {
                    result.Add(key: cols[1], new List<string>() { cols[0] });
                }
                else
                {
                    result[key: cols[1]].Add(cols[0]);
                }
            }

            return result;
        }

        private static Dictionary<string, List<string>> GetColumnDictionary(List<string> lines)
        {
            Dictionary<string, List<string>> result = new();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                cols[1] = cols[1].Replace("\"", "").Trim().ToUpper();

                if (!result.ContainsKey(cols[0]))
                {
                    result.Add(key: cols[0], new List<string>() { cols[1] });
                }
                else
                {
                    result[key: cols[0]].Add(cols[1]);
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

            using (SqlConnection connection = new($"Data Source={server};Initial Catalog={database};Integrated Security=True"))
            {
                SqlCommand command = new(queryString, connection);
                connection.Open();

                using SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    result.Add(string.Format("{0}, {1}", reader[0], reader[1]));
                }
            }

            return result;
        }
    }
}
