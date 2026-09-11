using System.Text;

namespace VontaGenta.Helpers
{
    public class DapperHelper
    {
        public string PrepareBulkInsertStatement(string table, List<string> colNames, int rows)
        {

            var sql = new StringBuilder();

            sql.Append($"INSERT INTO {table} (");
            for (int i = 0; i < colNames.Count; i++ )
            {
                sql.Append($"{colNames[i]}");
                if (i < colNames.Count -1)
                {
                    sql.Append(',');
                }
            }
            sql.Append(") VALUES ");

            for (int i = 0; i < rows; i++)
            { 
                sql.Append('(');
                for (int j = 0; j < colNames.Count; j++)
                {
                    sql.AppendFormat($"@{colNames[j]}{i}");
                    
                    if (j < colNames.Count - 1)
                    {
                        sql.Append(',');
                    }
                }

                sql.Append(')');

                if (i < rows - 1)
                {
                    sql.Append(',');
                }
            }

            return sql.ToString();
        }

    }
}
