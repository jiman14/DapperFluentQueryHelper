using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace LowCodingSoftware.DapperHelper
{
    public class DapperFluentQuery : DapperFluentQueryBase, IDapperFluentQuery
    {
        #region Query dapper

        public IEnumerable<T> Query<T>(IDbConnection connection) => connection.Query<T>(QueryStr, Parameters);
        public T QueryFirst<T>(IDbConnection connection) => connection.QueryFirst<T>(QueryStr, Parameters);

        #endregion

        #region Select commands

        public DapperFluentQuery Select(params string[] fields) =>
            Select(false, fields);
        public DapperFluentQuery SelectDistinct(params string[] fields) =>
            Select(true, fields);
        private DapperFluentQuery Select(bool distinct, params string[] fields)
        {
            Distinct = (fields.Length == 0) ? false: distinct;
            SelectFields = (fields.Length == 0)? "*": string.Join(",", fields.Where(f => !string.IsNullOrEmpty(f)));
            return this;
        }
        public DapperFluentQuery SelectCount(string columnName = "*")
        {
            SelectFields = $"COUNT({columnName})";
            return this;
        }

        #endregion

        #region From & Joins
        public DapperFluentQuery From<T>()
        {
            var modelType = typeof(T);
            ModelTypes.TryAdd(modelType.Name, modelType);
            FromClause = modelType.Name;
            return this;
        }

        /// <summary>
        /// Join: JOIN rightJoinFieldTable ON leftJoinField joinOperator rightJoinField. 
        /// </summary>
        /// <param name="leftJoinField"></param>
        /// <param name="joinOperator"></param>
        /// <param name="rightJoinField"></param>
        /// <returns></returns>
        public DapperFluentQuery Join<T>(string leftJoinField, JoinOperator joinOperator, string rightJoinField)
        => Join<T>(JoinType.Inner, new DapperFluentJoinFilter(leftJoinField, joinOperator, rightJoinField));
        public DapperFluentQuery LeftJoin<T>(string leftJoinField, JoinOperator joinOperator, string rightJoinField)
        => Join<T>(JoinType.Left, new DapperFluentJoinFilter(leftJoinField, joinOperator, rightJoinField));
        public DapperFluentQuery RightJoin<T>(string leftJoinField, JoinOperator joinOperator, string rightJoinField)
        => Join<T>(JoinType.Right, new DapperFluentJoinFilter(leftJoinField, joinOperator, rightJoinField));

        /// <summary>
        /// Join: JOIN rightJoinFieldTable ON (leftJoinField joinOperator rightJoinField AND () 
        /// FQ.Person.IdentifierTypeId, JoinOperator.Equals, FQ.IdentifierType.Id
        /// </summary>
        /// <param name="joinFilters"></param>
        /// <returns></returns>
        public DapperFluentQuery AddJoinFilter(bool AndOr, string leftJoinField, JoinOperator joinOperator, string rightJoinField)
        {
            FromClause += $"{(AndOr ? "AND":"OR")} {new DapperFluentJoinFilter(leftJoinField, joinOperator, rightJoinField).GetJoinFilter()}";
            return this;
        }

        private DapperFluentQuery Join<T>(JoinType joinType, DapperFluentJoinFilter joinFilter)
        {
            var modelType = typeof(T);
            ModelTypes.TryAdd(modelType.Name, modelType);
            FromClause += $"{joinFilter.GetJoin(joinType)} ON {joinFilter.GetJoinFilter()}";            
            return this;
        }
        #endregion

        #region Filter query

        public DapperFluentFilter Filter(string field, FilterOperator op, params object[] values)
            => FilterBase(field, op, values);

        public DapperFluentQuery Where(Func<DapperFluentQuery, DapperFluentFilters> where)
        {
            var filters = where.Invoke(this);
            return Where(filters.FiltersStr);
        }
        public DapperFluentQuery Where(Func<DapperFluentQuery, DapperFluentFilter> where)
        {
            var filter = where.Invoke(this);
            return Where(filter.CustomFilter);
        }                
        private DapperFluentQuery Where(string filtersStr)
        {
            WhereClause = string.IsNullOrEmpty(filtersStr)? string.Empty: $"WHERE {filtersStr}";
            return this;
        }

        /// <summary>
        /// Parenthesis: ( ... )
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        public DapperFluentFilters P(DapperFluentFilters filters) => new DapperFluentFilters($"({filters.FiltersStr})");
        /// <summary>
        /// Parenthesis Not: NOT ( ... )
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        public DapperFluentFilters PNot(DapperFluentFilters filters) => new DapperFluentFilters($"NOT ({filters.FiltersStr})");
        /// <summary>
        /// And filter join: filter1 AND filter2 AND ...
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        public DapperFluentFilters And(params DapperFluentFilter[] filters) => AndOr(true, filters);
        /// <summary>
        /// And filters join: (filter1 AND filter2) AND (filter3 OR filter4) ...
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        public DapperFluentFilters And(params DapperFluentFilters[] filters) => AndOr(true, filters);
        /// <summary>
        /// OR filter join: And: filter1 OR filter2 OR ...
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        public DapperFluentFilters Or(params DapperFluentFilters[] filters) => AndOr(false, filters);
        /// <summary>
        /// OR filters join: (filter1 AND filter2) OR (filter3 OR filter4) ...
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        public DapperFluentFilters Or(params DapperFluentFilter[] filters) => AndOr(false, filters);

        private DapperFluentFilters AndOr(bool and, params DapperFluentFilter[] filters)
            => new DapperFluentFilters(string.Join($" {(and ? "AND" : "OR")} ", filters.Where(f => !string.IsNullOrEmpty(f.CustomFilter)).Select(f => f.CustomFilter)));
        private DapperFluentFilters AndOr(bool and, params DapperFluentFilters[] filters)
            => new DapperFluentFilters(string.Join($" {(and ? "AND" : "OR")} ", filters.Where(f => !string.IsNullOrEmpty(f.FiltersStr)).Select(f => f.FiltersStr)));

        #endregion

        #region Group, order & having

        public DapperFluentQuery GroupBy(params string[] fileds)
        {
            GroupClause = fileds.Length == 0 ? string.Empty : $"GROUP BY {string.Join(",", fileds.Where(f => !string.IsNullOrEmpty(f)))}";
            return this;
        }

        /// <summary>
        /// Fill orderColumns param with direction intercalated with (Ascending or Descending), for ex.: ID,Ascending,CREATION_DATE,Descending 
        /// Or fill orderColumns param with direction intercalated with (true or false), for ex.: ID,true,CREATION_DATE,false 
        /// Or fill orderColumns param with direction intercalated with (asc or desc), for ex.: ID,asc,CREATION_DATE,desc
        /// In no direction is specified, default order is asc.
        /// </summary>
        /// <param name="orderFields"></param>
        /// <returns></returns>
        public DapperFluentQuery OrderBy(params string[] orderFields)
        {
            var sep = ",";
            var space = " ";
            var order = orderFields.Length == 0 ? string.Empty : $"ORDER BY {string.Join(sep, orderFields.Where(o => !string.IsNullOrEmpty(o)))}";
            OrderClause = order
                .Replace($"{sep}{SortDirection.Ascending}", $"{space}{QueryOrderBy.asc}")
                .Replace($"{sep}{SortDirection.Descending}", $"{space}{QueryOrderBy.desc}");
            OrderClause = OrderClause
                .Replace($"{sep}{bool.TrueString}", $"{space}{QueryOrderBy.asc}")
                .Replace($"{sep}{bool.FalseString}", $"{space}{QueryOrderBy.desc}");
            OrderClause = OrderClause
                .Replace($"{sep}{QueryOrderBy.asc}", $"{space}{QueryOrderBy.asc}")
                .Replace($"{sep}{QueryOrderBy.desc}", $"{space}{QueryOrderBy.desc}");
            return this;
        }

        public DapperFluentQuery Having(string expr, JoinOperator op, string value)
        {
            var having = op switch
            {
                JoinOperator.Distinct => $"{expr} <> '{value}' ",                
                JoinOperator.Mayor => $"{expr} > '{value}' ",
                JoinOperator.Minor => $"{expr} < '{value}' ",
                _=> $"{expr} = '{value}' "
            };
            HavingClause = $"HAVING ({having})";

            return this;
        }
        #endregion

        #region Limit query

        /// <summary>
        /// OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY
        /// </summary>
        /// <returns></returns>
        public DapperFluentQuery LimitOne() =>
            Limit(0, 1);
        /// <summary>
        /// OFFSET 0 ROWS FETCH NEXT {nRows} ROWS ONLY
        /// </summary>
        /// <param name="nRows"></param>
        /// <returns></returns>
        public DapperFluentQuery Limit(int nRows) =>
            Limit(0, nRows);
        /// <summary>
        /// OFFSET {offset} ROWS FETCH NEXT {nRows} ROWS ONLY
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="nRows"></param>
        /// <returns></returns>
        public DapperFluentQuery Limit(int offset, int nRows)
        {
            if (OrderClause == null)
                throw new InvalidOperationException("Dapper fluent Query error: ORDER BY clause is mandatory for fetching query results with pagination limits.");
            LimitClause += $"OFFSET {offset} ROWS FETCH NEXT {nRows} ROWS ONLY";
            return this;
        }

        #endregion
    }
}
