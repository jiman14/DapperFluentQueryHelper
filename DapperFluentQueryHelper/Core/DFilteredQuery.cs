using System;
using System.Linq;
using System.Linq.Expressions;

namespace DapperFluentQueryHelper.Core
{
    public class DFilteredQuery: DQuery
    {       
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