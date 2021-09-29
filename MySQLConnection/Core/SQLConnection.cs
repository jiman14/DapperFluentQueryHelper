using MySql.Data.MySqlClient;
using System.Data;

namespace MySQLConnection.Core
{
    public class SQLConnection
    {
        protected MySqlConnection Connection;
        protected int CommandTimeout = 30;
        private string ConnectionString { get; set; }
        public SQLConnection(string connectionString) => ConnectionString = connectionString;
        internal bool Open()
        {
            try
            {
                Connection = new MySqlConnection(ConnectionString);
                Connection.Open();
                return Connection.State == ConnectionState.Open;
            }
            catch (MySqlException ex)
            {
                throw new SQLException($"{nameof(MySQLConnection)}.{nameof(SQLConnection)}.{nameof(Open)}", ex);
            }
        }
        internal void Close()
        {
            try
            {
                if (Connection != null && Connection.State == System.Data.ConnectionState.Open)
                    Connection.Close();
            }
            catch (MySqlException ex)
            {
                throw new SQLException($"{nameof(MySQLConnection)}.{nameof(SQLConnection)}.{nameof(Close)}", ex);
            }
        }
    }
}