using Dapper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace LowCodingSoftware.FluentQuery
{
    public class DapperFluentQueryBase
    {
        #region Model types & query components & parameters

        protected ConcurrentDictionary<string, Type> ModelTypes = new ConcurrentDictionary<string, Type>();
        protected string SelectFields { get; set; } = string.Empty;
        protected string FromClause { get; set; }
        protected string WhereClause { get; set; }
        protected string GroupClause { get; set; }
        protected string HavingClause { get; set; }
        protected string OrderClause { get; set; }
        protected string LimitClause { get; set; }
        protected string DeleteQuery { get; set; }
        protected string UpdateQuery { get; set; }
        protected bool Distinct { get; set; } = false;

        protected DynamicParameters Parameters { get; private set; } = new DynamicParameters();
        private int ParameterIndex { get; set; } = 0;

        protected string QueryStr
            => $"SELECT {(Distinct ? "DISTINCT" : string.Empty)} {SelectFields} FROM {FromClause} {WhereClause} {GroupClause} {HavingClause} {OrderClause} {LimitClause}";
        #endregion
       
        internal DapperFluentFilter FilterBase(string field, FilterOperator op, params object[] values)
        {
            var filter = new DapperFluentFilter();
            if (!ModelTypes.TryGetValue(field.Split(".").First(), out Type modelType))
                throw new InvalidOperationException($"Dapper fluent filter field {field} not found in FROM clause");

            if (((values == null || values.Length == 0 || values[0] == null) && !(op == FilterOperator.IsNull || op == FilterOperator.NotNull)) &&
                ((string.IsNullOrEmpty(values[0]?.ToString()) && !(op == FilterOperator.Like || op == FilterOperator.NotLike)) ||
                (op == FilterOperator.Between && ((values.Length != 2) || string.IsNullOrEmpty(values[1]?.ToString())))))
                return filter;

            var pIndex = ParameterIndex + 1;
            filter.CustomFilter = op switch
            {
                FilterOperator.In => $"{field} in @P{pIndex} ",
                FilterOperator.NotIn => $"{field} not in @P{pIndex} ",
                FilterOperator.IsNull => $"{field} is Null ",
                FilterOperator.NotNull => $"{field} is not null ",
                FilterOperator.Distinct => $"{field} <> @P{pIndex} ",
                FilterOperator.Equals => $"{field} = @P{pIndex} ",
                FilterOperator.Like => $"{field} like @P{pIndex} ",
                FilterOperator.LikeFull => $"{field} like CONCAT('%', @P{pIndex}, '%') ",
                FilterOperator.EndWith => $"{field} like CONCAT('%', @P{pIndex}) ",
                FilterOperator.BeginWith => $"{field} like CONCAT(@P{pIndex}, '%') ",
                FilterOperator.NotLike => $"{field} not like @P{pIndex} ",
                FilterOperator.NotLikeFull => $"{field} not like CONCAT('%', @P{pIndex}, '%') ",
                FilterOperator.NotEndWith => $"{field} not like CONCAT('%', @P{pIndex}) ",
                FilterOperator.NotBeginWith => $"{field} not like CONCAT(@P{pIndex}, '%') ",
                FilterOperator.GreaterThan => $"{field} > @P{pIndex} ",
                FilterOperator.GreaterOrEqualsThan => $"{field} >= @P{pIndex} ",
                FilterOperator.LesserThan => $"{field} < @P{pIndex} ",
                FilterOperator.LesserOrEqualsThan => $"{field} <= @P{pIndex} ",
                FilterOperator.Between => $"{field} between @P{pIndex} and @P{++pIndex} ",
                _ => string.Empty
            };
            var paramNumber = op switch
            {
                FilterOperator.In => -1,
                FilterOperator.NotIn => -1,
                FilterOperator.IsNull => 0,
                FilterOperator.NotNull => 0,
                FilterOperator.Between => 2,
                _ => 1
            };
            if (paramNumber == -1)
                Parameters.Add($"P{++ParameterIndex}", values.ToList().First().ToString().StartsWith($"{nameof(System)}.{nameof(System.Collections)}")
                    ? values.ToList().First(): values);

            for (int i = 0; i < paramNumber; i++)
                Parameters.Add($"P{++ParameterIndex}", values[i], dbType: PropertiesTypeCache.GetPropertyType(modelType, field));

            return filter;
        }
    }
}
