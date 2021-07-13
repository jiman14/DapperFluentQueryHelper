using System;
using System.Collections.Generic;
using System.Data;

namespace LowCodingSoftware.DapperHelper
{
    public interface IDapperFluentQuery
    {
        IEnumerable<T> Query<T>(IDbConnection connection);
        T QueryFirst<T>(IDbConnection connection);
        DapperFluentQuery Select(params string[] columnNames);
        DapperFluentQuery SelectDistinct(params string[] columnNames);
        DapperFluentQuery SelectCount(string columnName = "*");
        DapperFluentQuery From<T>();
        DapperFluentQuery Join<T>(string leftJoinField, JoinOperator joinOperator, string rightJoinField);
        DapperFluentQuery LeftJoin<T>(string leftJoinField, JoinOperator joinOperator, string rightJoinField);
        DapperFluentQuery RightJoin<T>(string leftJoinField, JoinOperator joinOperator, string rightJoinField);
        DapperFluentQuery AddJoinFilter(bool AndOr, string leftJoinField, JoinOperator joinOperator, string rightJoinField);
        DapperFluentQuery Where(Func<DapperFluentQuery, DapperFluentFilters> where);
        DapperFluentQuery Where(Func<DapperFluentQuery, DapperFluentFilter> where);        
        DapperFluentFilter Filter(string field, FilterOperator op, params object[] values);
        DapperFluentFilters P(DapperFluentFilters filters);
        DapperFluentFilters And(params DapperFluentFilter[] filters);
        DapperFluentFilters And(params DapperFluentFilters[] filters);
        DapperFluentFilters Or(params DapperFluentFilter[] filters);
        DapperFluentFilters Or(params DapperFluentFilters[] filters);
        DapperFluentQuery GroupBy(params string[] fileds);
        DapperFluentQuery OrderBy(params string[] orderColumns);
        DapperFluentQuery Having(string expr, JoinOperator op, string value);
        DapperFluentQuery LimitOne();
        DapperFluentQuery Limit(int nRows);
        DapperFluentQuery Limit(int offset, int nRows);
    }    
}
