﻿using System;
using System.Linq;
using System.Linq.Expressions;

namespace DapperFluentQueryHelper.Core
{
    public class DFilteredQuery: DQuery
    {
        public string LastAndOr { get; set; } = "";
        public string CurrAndOr { get; set; } = "";
        public DapperFluentFilter F(string field, params object[] values)
            => FilterBase(field, FilterOperator.Equals, values);
        public DapperFluentFilter F(string field, FilterOperator op, params object[] values)
            => FilterBase(field, op, values);
        public DapperFluentFilter FIn(string field, params object[] values)
            => FilterBase(field, FilterOperator.In, values);
        public DapperFluentFilter FBetWeen(string field, object valueMin, object valueMax)
            => FilterBase(field, FilterOperator.Between, valueMin, valueMax);
        public DapperFluentFilter FIsNull(string field)
            => FilterBase(field, FilterOperator.IsNull);
        public DapperFluentFilter FNotIsNull(string field)
            => FilterBase(field, FilterOperator.NotNull);
        public DapperFluentFilter F(DapperFluentFilter funcFilter)
            => new DapperFluentFilter($"({funcFilter.CustomFilter})");

        /// <summary>
        /// And filters join: (filter1 AND filter2) AND (filter3 OR filter4) ...
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        public DapperFluentFilter And(params DapperFluentFilter[] filters)
        { CurrAndOr = "AND"; return AndOr(true, filters); }
        /// <summary>
        /// OR filter join: And: filter1 OR filter2 OR ...
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        public DapperFluentFilter Or(params DapperFluentFilter[] filters)
        { CurrAndOr = "OR";  return AndOr(false, filters); }

        private DapperFluentFilter AndOr(bool and, params DapperFluentFilter[] filters)
            => new DapperFluentFilter((filters.Where(f => !string.IsNullOrEmpty(f?.CustomFilter)).Any() ? "(" : "") +
                string.Join($" {(and ? "AND" : "OR")} ", filters.Where(f => !string.IsNullOrEmpty(f.CustomFilter)).Select(f => f.CustomFilter)) +
                (filters.Where(f => !string.IsNullOrEmpty(f?.CustomFilter)).Any() ? ")" : ""));

        public void FillWhereClause(string filtersStr)
        {
            if (!string.IsNullOrEmpty(WhereClause) && !string.IsNullOrEmpty(filtersStr))
                WhereClause = $"({WhereClause} {LastAndOr} {filtersStr})";
            else if (!string.IsNullOrEmpty(filtersStr))
                WhereClause = filtersStr;
            LastAndOr = CurrAndOr;
        }
    }
}