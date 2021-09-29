﻿using Dapper;
using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace DapperFluentQueryHelper.Core
{
    public class DDelete : DFilteredQuery
    {
        public DDelete Delete<T>()
        {
            CommandType = CommandTypes.delete;
            var modelType = typeof(T);
            TableName = modelType.Name;
            ModelTypes.TryAdd(modelType.Name, modelType);
            return this;
        }
        public DDelete Where(Func<DFilteredQuery, DapperFluentFilters> where)
        {
            var filters = where.Invoke(this);
            return Where(filters.FiltersStr);
        }
        public DDelete Where(Func<DFilteredQuery, DapperFluentFilter> where)
        {
            var filter = where.Invoke(this);
            return Where(filter.CustomFilter);
        }
        private DDelete Where(string filtersStr)
        {
            WhereClause = string.IsNullOrEmpty(filtersStr) ? string.Empty : $"WHERE {filtersStr}";
            return this;
        }
    }
}