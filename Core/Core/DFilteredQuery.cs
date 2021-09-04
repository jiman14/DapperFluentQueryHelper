using System;
using System.Linq;
using System.Linq.Expressions;

namespace DapperFluentQueryHelper.Core
{
    public class DFilteredQuery: DQueryBase, IDFilteredQuery
    {
        public DapperFluentFilter Filter<T>(Expression<Func<T>> expression)
        {
            if (!(expression?.Body is BinaryExpression)) throw new ArgumentException("Must be a binary expression, i.e. entity.prop1 == 2");
            var be = expression?.Body as BinaryExpression;
            var op = be.NodeType == ExpressionType.Equal ? FilterOperator.Equals :
                be.NodeType == ExpressionType.NotEqual ? FilterOperator.Distinct :
                be.NodeType == ExpressionType.GreaterThan ? FilterOperator.GreaterThan :
                be.NodeType == ExpressionType.LessThan ? FilterOperator.LesserThan :
                be.NodeType == ExpressionType.GreaterThanOrEqual ? FilterOperator.GreaterThanOrEqual :
                be.NodeType == ExpressionType.LessThanOrEqual ? FilterOperator.LessThanOrEqual :
                throw new ArgumentException("Binary operators allowed: ==, !=, >, <, <=, >=");
            return FilterBase(GetFieldFromBinaryExpression(be), op, GetValueFromBinaryExpression(be));
        }
        private string GetFieldFromBinaryExpression(BinaryExpression be)
        {
            if (be.Left is MemberExpression) return GetFieldFromExpression(be.Left as MemberExpression);
            else if (be.Right is MemberExpression) return GetFieldFromExpression(be.Right as MemberExpression);
            throw new ArgumentException("BinaryExpression must have one MemberExpression");
        }        
        private object GetValueFromBinaryExpression(BinaryExpression be)
        {
            if (be.Left is ConstantExpression) return (be.Left as ConstantExpression).Value;
            else if (be.Right is ConstantExpression) return (be.Right as ConstantExpression).Value;
            throw new ArgumentException("BinaryExpression must have one ConstantExpression");
        }
        private string GetFieldFromExpression(MemberExpression op) => $"{op.Member.DeclaringType.Name}.{op.Member.Name}";        
        public DapperFluentFilter Filter(string field, FilterOperator op, params object[] values)
            => FilterBase(field, op, values);
        public DapperFluentFilter FilterIn(string field, params object[] values)
            => FilterBase(field, FilterOperator.In, values);
        public DapperFluentFilter FilterBetWeen(string field, object valueMin, object valueMax)
            => FilterBase(field, FilterOperator.Between, valueMin, valueMax);
        public DapperFluentFilter FilterIsNull(string field)
            => FilterBase(field, FilterOperator.IsNull);
        public DapperFluentFilter FilterNotIsNull(string field)
            => FilterBase(field, FilterOperator.NotNull);
        public IDFilteredQuery Where(Func<IDFilteredQuery, DapperFluentFilters> where)
        {
            var filters = where.Invoke(this);
            return Where(filters.FiltersStr);
        }
        public IDFilteredQuery Where(Func<IDFilteredQuery, DapperFluentFilter> where)
        {
            var filter = where.Invoke(this);
            return Where(filter.CustomFilter);
        }
        private IDFilteredQuery Where(string filtersStr)
        {
            WhereClause = string.IsNullOrEmpty(filtersStr) ? string.Empty : $"WHERE {filtersStr}";
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
    }
}