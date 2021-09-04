using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace DapperFluentQueryHelper.Core
{
    public interface IDQuery
    {
        IEnumerable<T> Query<T>(IDbConnection connection);
        T QueryFirst<T>(IDbConnection connection);
        DQuery Select(params string[] columnNames);
        DQuery Select<T>(Expression<Func<T>> expression);
        DQuery SelectDistinct(params string[] columnNames);
        DQuery SelectCount(string columnName = "*");
        DQuery From<T>();
        DQuery Join<T>(string leftJoinField, JoinOperator joinOperator, string rightJoinField);
        DQuery LeftJoin<T>(string leftJoinField, JoinOperator joinOperator, string rightJoinField);
        DQuery RightJoin<T>(string leftJoinField, JoinOperator joinOperator, string rightJoinField);
        DQuery AddJoinFilter(bool AndOr, string leftJoinField, JoinOperator joinOperator, string rightJoinField);
        DQuery GroupBy(params string[] fileds);
        DQuery OrderBy(params string[] orderColumns);
        DQuery Having(string expr, JoinOperator op, string value);
        DQuery LimitOne();
        DQuery Limit(int nRows);
        DQuery Limit(int offset, int nRows);
    }    
}
