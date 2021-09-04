using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace DapperFluentQueryHelper.Core
{
    public class DQuery : DFilteredQuery, IDQuery
    {
        #region Query dapper

        public IEnumerable<T> Query<T>(IDbConnection connection) => connection.Query<T>(QueryStr, Parameters);
        public T QueryFirst<T>(IDbConnection connection) => connection.QueryFirst<T>(QueryStr, Parameters);

        #endregion

        #region Select commands

        public DQuery Select<T>(Expression<Func<T>> expression) =>
            Select(false, ((MemberInitExpression)expression?.Body)?.Bindings?.Select(p => p.Member.Name));
        public DQuery Select(params string[] fields) =>
            Select(false, fields);
        public DQuery SelectDistinct(params string[] fields) =>
            Select(true, fields);
        private DQuery Select(bool distinct, IEnumerable<string> fields)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            Distinct = (fields.Count() == 0) ? false: distinct;
            SelectFields = (fields.Count() == 0)? "*": string.Join(",", fields.Where(f => !string.IsNullOrEmpty(f)));
            return this;
        }
        public DQuery SelectCount(string columnName = "*")
        {
            SelectFields = $"COUNT({columnName})";
            return this;
        }

        #endregion

        #region From & Joins
        public DQuery From<T>()
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
        public DQuery Join<T>(string leftJoinField, JoinOperator joinOperator, string rightJoinField)
        => Join<T>(JoinType.Inner, new DapperFluentJoinFilter(leftJoinField, joinOperator, rightJoinField));
        public DQuery LeftJoin<T>(string leftJoinField, JoinOperator joinOperator, string rightJoinField)
        => Join<T>(JoinType.Left, new DapperFluentJoinFilter(leftJoinField, joinOperator, rightJoinField));
        public DQuery RightJoin<T>(string leftJoinField, JoinOperator joinOperator, string rightJoinField)
        => Join<T>(JoinType.Right, new DapperFluentJoinFilter(leftJoinField, joinOperator, rightJoinField));

        /// <summary>
        /// Join: JOIN rightJoinFieldTable ON (leftJoinField joinOperator rightJoinField AND () 
        /// FQ.Person.IdentifierTypeId, JoinOperator.Equals, FQ.IdentifierType.Id
        /// </summary>
        /// <param name="joinFilters"></param>
        /// <returns></returns>
        public DQuery AddJoinFilter(bool AndOr, string leftJoinField, JoinOperator joinOperator, string rightJoinField)
        {
            FromClause += $"{(AndOr ? "AND":"OR")} {new DapperFluentJoinFilter(leftJoinField, joinOperator, rightJoinField).GetJoinFilter()}";
            return this;
        }

        private DQuery Join<T>(JoinType joinType, DapperFluentJoinFilter joinFilter)
        {
            var modelType = typeof(T);
            ModelTypes.TryAdd(modelType.Name, modelType);
            FromClause += $"{joinFilter.GetJoin(joinType)} ON {joinFilter.GetJoinFilter()}";            
            return this;
        }
        #endregion

        #region Group, order & having

        public DQuery GroupBy(params string[] fileds)
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
        public DQuery OrderBy(params string[] orderFields)
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

        public DQuery Having(string expr, JoinOperator op, string value)
        {
            var having =
            (
                op == JoinOperator.Distinct ? $"{expr} <> '{value}' " :
                op == JoinOperator.Mayor ? $"{expr} > '{value}' " :
                op == JoinOperator.Minor ? $"{expr} < '{value}' " :
                $"{expr} = '{value}' "
            );            
            HavingClause = $"HAVING ({having})";

            return this;
        }
        #endregion

        #region Limit query

        /// <summary>
        /// OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY
        /// </summary>
        /// <returns></returns>
        public DQuery LimitOne() =>
            Limit(0, 1);
        /// <summary>
        /// OFFSET 0 ROWS FETCH NEXT {nRows} ROWS ONLY
        /// </summary>
        /// <param name="nRows"></param>
        /// <returns></returns>
        public DQuery Limit(int nRows) =>
            Limit(0, nRows);
        /// <summary>
        /// OFFSET {offset} ROWS FETCH NEXT {nRows} ROWS ONLY
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="nRows"></param>
        /// <returns></returns>
        public DQuery Limit(int offset, int nRows)
        {
            if (OrderClause == null)
                throw new InvalidOperationException("Dapper fluent Query error: ORDER BY clause is mandatory for fetching query results with pagination limits.");
            LimitClause += $"OFFSET {offset} ROWS FETCH NEXT {nRows} ROWS ONLY";
            return this;
        }

        #endregion
    }
}
