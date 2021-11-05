using System;
using System.Data;
using System.Data.SQLite;

namespace WpfSystemMonitor
{
    public static class DB
    {
        private static SQLiteConnection sqliteConnection;

        private static SQLiteConnection DbConnection()
        {
            sqliteConnection = new SQLiteConnection(@"Data Source=C:\\Tecfy2SE\\Log.db3;Version=3; Version=3;");
            if (sqliteConnection.State == ConnectionState.Closed)
                sqliteConnection.Open();

            return sqliteConnection;
        }

        public static DataTable GetLogs()
        {            
            try
            {
                using (var cmd = DbConnection().CreateCommand())
                {
                    DataTable dt = new DataTable();
                    cmd.CommandText = "SELECT * FROM Log";
                    var da = new SQLiteDataAdapter(cmd);
                    da.Fill(dt);
                    return dt;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static DataTable DeleteLogs()
        {
            try
            {
                using (var cmd = DbConnection().CreateCommand())
                {
                    DataTable dt = new DataTable();
                    cmd.CommandText = "Delete FROM Log";
                    var da = new SQLiteDataAdapter(cmd);
                    da.Fill(dt);
                    return dt;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
