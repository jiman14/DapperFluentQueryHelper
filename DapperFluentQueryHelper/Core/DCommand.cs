using Dapper;
using Dapper.Contrib.Extensions;
using DapperFluentQueryHelper.Core.Serializer;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DapperFluentQueryHelper.Core
{
    public class DCommand
    {
        private IDbConnection Connection;
        private IDbTransaction Transaction;
        public DCommand(IDbConnection connection, IDbTransaction transaction = null)
        {
            Connection = connection;
            Transaction = transaction;

        }
        public T Get<T>(int id) where T : BaseModel => Connection?.Get<T>(id, Transaction, commandTimeout: Connection?.ConnectionTimeout);
        public T Get<T>(string id) where T : BaseModel => Connection?.Get<T>(id, Transaction, commandTimeout: Connection?.ConnectionTimeout);
        public IEnumerable<T> GetAll<T>() where T : BaseModel => Connection?.GetAll<T>(Transaction, commandTimeout: Connection?.ConnectionTimeout);
        public IEnumerable<T> Query<T>(DSelect select) => Connection.Query<T>(select.QueryStr, select.Parameters, Transaction, commandTimeout: Connection?.ConnectionTimeout);
        public IEnumerable<T> Query<T>(SerializableQuery serializedQuery)
        {
            var query = DeserializedQuery.Get(serializedQuery);
            return Connection.Query<T>(query.Query, query.Parameters, Transaction, commandTimeout: Connection?.ConnectionTimeout);
        }
        public T QueryOne<T>(DSelect select) => Connection.Query<T>(select.QueryStr, select.Parameters, Transaction, commandTimeout: Connection?.ConnectionTimeout).FirstOrDefault();
        public T QueryOne<T>(SerializableQuery serializedQuery)
        {
            var query = DeserializedQuery.Get(serializedQuery);
            return Connection.Query<T>(query.Query, query.Parameters, Transaction, commandTimeout: Connection?.ConnectionTimeout).FirstOrDefault();
        }
        public bool Update<T>(T obj) where T : BaseModel => Connection?.Update(obj, Transaction, commandTimeout: Connection?.ConnectionTimeout) ?? false;
        public bool Update<T>(IEnumerable<T> objs) where T : BaseModel => Connection?.Update(objs, Transaction, commandTimeout: Connection?.ConnectionTimeout) ?? false;
        public int Update(DUpdate updateQuery) => Connection?.Execute(updateQuery.UpdateStr, updateQuery.Parameters, Transaction, commandTimeout: Connection?.ConnectionTimeout) ?? -1;

        public long Insert<T>(T obj) where T : BaseModel => Connection?.Insert(obj, Transaction, commandTimeout: Connection?.ConnectionTimeout) ?? -1;
        public long Insert<T>(IEnumerable<T> objs) where T : BaseModel => Connection?.Insert(objs, Transaction, commandTimeout: Connection?.ConnectionTimeout) ?? -1;

        public bool Delete<T>(T obj) where T : BaseModel => Connection?.Delete(obj, Transaction, commandTimeout: Connection?.ConnectionTimeout) ?? false;
        public bool Delete<T>(IEnumerable<T> objs) where T : BaseModel => Connection?.Delete(objs, Transaction, commandTimeout: Connection?.ConnectionTimeout) ?? false;
        public bool DeleteAll<T>() where T : BaseModel => Connection?.DeleteAll<T>(Transaction, commandTimeout: Connection?.ConnectionTimeout) ?? false;
        public int Delete(DDelete delete) => Connection.Execute(delete.DeleteStr, delete.Parameters, Transaction, commandTimeout: Connection?.ConnectionTimeout);
    }
}
