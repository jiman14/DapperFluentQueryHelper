using System;

namespace DapperFluentQueryHelper.Core
{
    public class DDelete : DFilteredQuery
    {
        public DDelete Delete<T>()
        {
            CommandType = CommandTypes.delete;
            var modelType = typeof(T);
            TableName = modelType.Name;
            ModelTypes.TryAdd(modelType.Name, modelType);
            return this;
        }
        public DDelete From<T>()
        {
            var modelType = typeof(T);
            ModelTypes.TryAdd(modelType.Name, modelType);
            return From(modelType.Name);
        }
        public DDelete From(string from)
        {
            FromClause = from;
            return this;
        }

        public DDelete Where(Func<DFilteredQuery, DapperFluentFilter> where)
        {
            var filter = where.Invoke(this);
            return Where(filter.CustomFilter);
        }
        private DDelete Where(string filtersStr)
        {
            FillWhereClause(filtersStr);
            return this;
        }
    }
}