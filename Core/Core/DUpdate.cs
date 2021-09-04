using Dapper;
using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace DapperFluentQueryHelper.Core
{
    public class DUpdate : DFilteredQuery, IDUpdate
    {
        public bool Update<T>(IDbConnection connection) => connection.Execute(QueryStr, Parameters) > 0;
        public IDUpdate Update<T>(Expression<Func<T>> expression)
        {
            UpdateFields = string.Join(", ", ((MemberInitExpression)expression?.Body)?.Bindings.Select(p =>
                AddUpdateProperty<T>($"{p.Member.DeclaringType.Name}.{p.Member.Name}", ((ConstantExpression)((MemberAssignment)p).Expression).Value)));
            return this;
        }        
    }
}