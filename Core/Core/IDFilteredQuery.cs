using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace DapperFluentQueryHelper.Core
{
    public interface IDFilteredQuery
    {
        IDFilteredQuery Where(Func<IDFilteredQuery, DapperFluentFilters> where);
        IDFilteredQuery Where(Func<IDFilteredQuery, DapperFluentFilter> where);
        DapperFluentFilter Filter<T>(Expression<Func<T>> expression);
        DapperFluentFilter Filter(string field, FilterOperator op, params object[] values);
        DapperFluentFilter FilterIn(string field, params object[] values);
        DapperFluentFilter FilterBetWeen(string field, object valueMin, object valueMax);
        DapperFluentFilter FilterIsNull(string field);
        DapperFluentFilter FilterNotIsNull(string field);
        DapperFluentFilters P(DapperFluentFilters filters);
        DapperFluentFilters And(params DapperFluentFilter[] filters);
        DapperFluentFilters And(params DapperFluentFilters[] filters);
        DapperFluentFilters Or(params DapperFluentFilter[] filters);
        DapperFluentFilters Or(params DapperFluentFilters[] filters);
    }    
}
