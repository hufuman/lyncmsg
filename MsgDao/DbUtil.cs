using System;
using System.Data.SQLite;

namespace MsgDao
{
    public class DbUtil
    {
        static public SQLiteDataReader ExecuteSql(SQLiteConnection sqlCnn, string sql, SQLiteParameter[] parameters)
        {
            SQLiteCommand command = null;
            SQLiteDataReader reader = null;
            try
            {
                command = new SQLiteCommand(sql, sqlCnn);
                if (parameters != null)
                    command.Parameters.AddRange(parameters);
                reader = command.ExecuteReader();
                command.Dispose();
                return reader;
            }
            catch (Exception e)
            {
                if (reader != null)
                    reader.Close();
                if (command != null)
                    command.Dispose();
                Console.WriteLine("ExecuteSql 失败，原因：" + e.Message);
                return null;
            }
        }

        static public object ExecuteSqlReturnValue(SQLiteConnection sqlCnn, string sql,
            SQLiteParameter[] parameters)
        {
            var reader = ExecuteSql(sqlCnn, sql, parameters);
            if (reader == null)
                return null;
            object result = null;
            if (reader.Read())
            {
                result = reader.GetValue(0);
            }
            reader.Close();
            return result;
        }

        public static bool ExecuteSqlNoQuery(SQLiteConnection sqlCnn, string sql,
            SQLiteParameter[] parameters)
        {
            SQLiteCommand command = null;
            try
            {
                command = new SQLiteCommand(sql, sqlCnn);
                if (parameters != null)
                    command.Parameters.AddRange(parameters);
                command.ExecuteNonQuery();
                command.Dispose();
                return true;
            }
            catch (Exception e)
            {
                if (command != null)
                    command.Dispose();
                Console.WriteLine("ExecuteSqlNoQuery 失败，原因：" + e.Message);
                return false;
            }
        }

        public static SQLiteParameter BuildParameter(string name, System.Data.DbType dbType, object value)
        {
            var result = new SQLiteParameter(dbType)
            {
                Value = value,
                ParameterName = name,
            };
            return result;
        }
    }
}
