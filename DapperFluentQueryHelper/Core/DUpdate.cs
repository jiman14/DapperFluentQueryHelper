using System;
using System.Linq;
using System.Linq.Expressions;

namespace DapperFluentQueryHelper.Core
{
    public class DUpdate : DFilteredQuery
    {
        public DUpdate Update<T>(Expression<Func<T>> expression)
        {
            UpdateFields = string.Join(", ", ((MemberInitExpression)expression?.Body)?.Bindings.Select(p =>
                AddUpdateProperty<T>($"{p.Member.DeclaringType.Name}.{p.Member.Name}", ((ConstantExpression)((MemberAssignment)p).Expression).Value)));
            CommandType = CommandTypes.update;
            var modelType = typeof(T);
            TableName = modelType.Name;
            ModelTypes.TryAdd(modelType.Name, modelType);
            return this;
        }
        public DUpdate Where(Func<DFilteredQuery, DapperFluentFilter> where)
        {
            var filter = where.Invoke(this);
            return Where(filter.CustomFilter);
        }
        private DUpdate Where(string filtersStr)
        {
            WhereClause = string.IsNullOrEmpty(filtersStr) ? string.Empty : $"WHERE {filtersStr}";
            return this;
        }
    }
}