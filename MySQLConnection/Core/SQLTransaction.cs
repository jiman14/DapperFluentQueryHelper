using DapperFluentQueryHelper.Core;
using MySql.Data.MySqlClient;
using System;
using System.Runtime.CompilerServices;

namespace MySQLConnection.Core
{
    public class SQLTransaction: SQLConnection
    {
        MySqlTransaction Trans;
        public SQLTransaction(string connectionString) : base(connectionString) { }
        public R NonTransaction<R>(Func<DCommand, R> func, [CallerMemberName] string callerMemberName = "")
        {
            R res = default(R);
            try
            {
                if (Open())
                {
                    var DCommand = new DCommand(Connection);
                    res = func.Invoke(DCommand);
                }
            }
            catch (MySqlException ex)
            {
                throw new SQLException($"{callerMemberName} => {nameof(MySQLConnection)}.{nameof(SQLTransaction)}.{nameof(NonTransaction)}", ex);
            }
            finally
            {
                Close();
            }
            return res;
        }
        public R Transaction<R>(Func<DCommand, R> func, [CallerMemberName] string callerMemberName = "")
        {
            R res = default(R);
            try
            {
                OpenTransaction();
                var DCommand = new DCommand(Connection, Trans);
                res = func.Invoke(DCommand);
                Trans.Commit();
            }
            catch (MySqlException ex)
            {                
                if (Trans != null)
                    Rollback();
                throw new SQLException($"{callerMemberName} => {nameof(MySQLConnection)}.{nameof(SQLTransaction)}.{nameof(Transaction)}", ex);
            }
            finally
            {
                Close();
            }
            return res;
        }
        public bool Rollback([CallerMemberName] string callerMemberName = "")
        {
            try
            {
                Trans.Rollback();
            }
            catch (MySqlException ex)
            {
                throw new SQLException($"{callerMemberName} => {nameof(MySQLConnection)}.{nameof(SQLTransaction)}.{nameof(Rollback)}", ex);
            }
            return true;
        }
        private bool OpenTransaction()
        {
            try
            {
                if (Open())
                {
                    Trans = Connection.BeginTransaction();
                    return true;
                }
                return false;
            }
            catch (MySqlException ex)
            {
                throw new SQLException($"{nameof(MySQLConnection)}.{nameof(SQLTransaction)}.{nameof(OpenTransaction)}", ex);
            }
        }        
    }
}