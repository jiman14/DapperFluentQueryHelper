using System;
using System.Collections.Generic;
using System.Linq;

namespace DapperFluentQueryHelper.Core
{
    public class DSelect : DFilteredQuery
    {
        #region Select commands

        public DSelect SelectAll() =>
            Select(false, new string[]{});
        public DSelect Select(params string[] fields) =>
            Select(false, fields);
        public DSelect SelectDistinct(params string[] fields) =>
            Select(true, fields);
        private DSelect Select(bool distinct, IEnumerable<string> fields)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            Distinct = (fields.Count() == 0) ? false: distinct;
            if (fields.Count() > 0 && fields.First().Contains("."))
            {
                var fieldParts = fields.First().Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                FromClause = fields.First().Replace($".{fieldParts[fieldParts.Count() - 1]}", string.Empty);
            }
            SelectFields = (fields.Count() == 0)? "*": string.Join(",", fields.Where(f => !string.IsNullOrEmpty(f)));
            return this;
        }
        public DSelect SelectCount(string columnName = "*")
        {
            SelectFields = $"COUNT({columnName})";
            return this;
        }
        public DSelect WhereAnd(Func<DFilteredQuery, DapperFluentFilter> where)
        => Where(where, true);
        public DSelect WhereOr(Func<DFilteredQuery, DapperFluentFilter> where)
        => Where(where, false);
        public DSelect Where(Func<DFilteredQuery, DapperFluentFilter> where)
        => Where(where, null);
        private DSelect Where(Func<DFilteredQuery, DapperFluentFilter> where, bool? andOr)
        {
            if (andOr.HasValue)
                LastAndOr = andOr.Value ? "AND" : "OR";
            var filter = where.Invoke(this);
            return Where(filter.CustomFilter);
        }
        private DSelect Where(string filtersStr)
        {
            FillWhereClause(filtersStr);
            return this;
        }
        #endregion

        #region From & Joins
        public DSelect From<T>()
        {
            var modelType = typeof(T);
            ModelTypes.TryAdd(modelType.Name, modelType);
            return From(modelType.Name);
        }
        public DSelect From(string from)
        {
            FromClause = from;
            return this;
        }

        /// <summary>
        /// Join: JOIN rightJoinFieldTable ON leftJoinField joinOperator rightJoinField. 
        /// </summary>
        /// <param name="leftJoinField"></param>
        /// <param name="joinOperator"></param>
        /// <param name="rightJoinField"></param>
        /// <returns></returns>
        public DSelect Join<T>(string leftJoinField, JoinOperator joinOperator, string rightJoinField)
        => Join<T>(JoinType.Inner, new DapperFluentJoinFilter(leftJoinField, joinOperator, rightJoinField));
        public DSelect LeftJoin<T>(string leftJoinField, JoinOperator joinOperator, string rightJoinField)
        => Join<T>(JoinType.Left, new DapperFluentJoinFilter(leftJoinField, joinOperator, rightJoinField));
        public DSelect RightJoin<T>(string leftJoinField, JoinOperator joinOperator, string rightJoinField)
        => Join<T>(JoinType.Right, new DapperFluentJoinFilter(leftJoinField, joinOperator, rightJoinField));

        /// <summary>
        /// Join: JOIN rightJoinFieldTable ON (leftJoinField joinOperator rightJoinField AND () 
        /// FQ.Person.IdentifierTypeId, JoinOperator.Equals, FQ.IdentifierType.Id
        /// </summary>
        /// <param name="joinFilters"></param>
        /// <returns></returns>
        public DSelect AddJoinFilter(bool AndOr, string leftJoinField, JoinOperator joinOperator, string rightJoinField)
        {
            FromClause += $"{(AndOr ? "AND":"OR")} {new DapperFluentJoinFilter(leftJoinField, joinOperator, rightJoinField).GetJoinFilter()}";
            return this;
        }

        private DSelect Join<T>(JoinType joinType, DapperFluentJoinFilter joinFilter)
        {
            var modelType = typeof(T);
            ModelTypes.TryAdd(modelType.Name, modelType);
            FromClause += $"{joinFilter.GetJoin(joinType)} ON {joinFilter.GetJoinFilter()}";            
            return this;
        }
        #endregion

        #region Group, order & having

        public DSelect GroupBy(params string[] fileds)
        {
            GroupClause = !fileds.Any() ? string.Empty : $"{string.Join(",", fileds.Where(f => !string.IsNullOrEmpty(f)))}";
            return this;
        }
        public DSelect OrderBy(string orderField) => OrderBy(orderField, true);
        public DSelect OrderByDesc(string orderField) => OrderBy(orderField, false);
        public DSelect ThenOrderBy(string orderField) => OrderBy(orderField, true);
        public DSelect ThenOrderByDesc(string orderField) => OrderBy(orderField, false);
        private DSelect OrderBy(string orderField, bool orderAsc)
        {
            OrderClause = (string.IsNullOrEmpty(OrderClause)) ?
                $"{orderField} {(orderAsc ? QueryOrderBy.asc : QueryOrderBy.desc)}" :
                $"{OrderClause}, {orderField} {(orderAsc ? QueryOrderBy.asc : QueryOrderBy.desc)}";                
            return this;
        }

        public DSelect Having(string expr, JoinOperator op, string value)
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
        public DSelect LimitOne() =>
            Limit(0, 1);
        /// <summary>
        /// OFFSET 0 ROWS FETCH NEXT {nRows} ROWS ONLY
        /// </summary>
        /// <param name="nRows"></param>
        /// <returns></returns>
        public DSelect Limit(int nRows) =>
            Limit(0, nRows);
        /// <summary>
        /// OFFSET {offset} ROWS FETCH NEXT {nRows} ROWS ONLY
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="nRows"></param>
        /// <returns></returns>
        public DSelect Limit(int offset, int nRows)
        {
            if (string.IsNullOrEmpty(OrderClause))
                OrderClause = "ORDER BY Id";
                
            LimitClause += $"Limit {offset}, {nRows}";
            return this;
        }

        #endregion
    }
}
