﻿using Dapper;
using DapperFluentQueryHelper.Core.Serializer;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;

namespace DapperFluentQueryHelper.Core
{
    public class DQuery
    {       
        #region Model types & query components & parameters
        protected enum CommandTypes
        {
            select,
            update,
            delete
        }
        protected CommandTypes CommandType = CommandTypes.select;
        protected ConcurrentDictionary<string, Type> ModelTypes = new ConcurrentDictionary<string, Type>();
        protected string SelectFields { get; set; } = string.Empty;
        protected string UpdateFields { get; set; } = string.Empty;        
        protected string TableName { get; set; }
        protected string FromClause { get; set; }
        protected string WhereClause { get; set; }        
        protected string GroupClause { get; set; }
        protected string HavingClause { get; set; }
        protected string OrderClause { get; set; }
        protected string LimitClause { get; set; }
        protected string DeleteQuery { get; set; }
        protected string UpdateQuery { get; set; }
        protected bool Distinct { get; set; } = false;

        internal DynamicParameters Parameters { get; private set; } = new DynamicParameters();
        private int ParameterIndex { get; set; } = 0;

        public string QueryStr
            => $@"SELECT {(Distinct ? "DISTINCT" : string.Empty)} {SelectFields} 
                  FROM {FromClause} 
                  {(string.IsNullOrEmpty(WhereClause) ? "" : "WHERE ")}{WhereClause}
                  {(string.IsNullOrEmpty(GroupClause) ? "" : "GROUP BY ")}{GroupClause} 
                  {HavingClause} 
                  {(string.IsNullOrEmpty(OrderClause) ? "" : "ORDER BY ")}{OrderClause} 
                  {LimitClause}"
            .Replace("__", ".");
        public string UpdateStr
            => $"UPDATE {TableName} SET {UpdateFields} {(string.IsNullOrEmpty(WhereClause) ? "" : "WHERE ")}{WhereClause}"
            .Replace("__", ".");
        public string DeleteStr
            => $"DELETE FROM {FromClause} {(string.IsNullOrEmpty(WhereClause) ? "" : "WHERE ")}{WhereClause}"
            .Replace("__", ".");

        public string DebugQuery()
        {
            var deserialized = SerializeSelect();
            var i = deserialized.Parameters.Count()+1;
            var query = deserialized.Query;
            foreach (var param in deserialized.Parameters.OrderByDescending(p => int.Parse(p.Name.Substring(1))))
            {
                string value = $"'{param.Value}'";
                if (param.ParamType.ToLower().Contains("int") || param.ParamType.ToLower().Contains("decimal"))
                    value = $"{param.Value}";
                else if (param.ParamType.ToLower().Contains("bool"))
                    value = $"{param.Value.ToLower()}";
                query = query.Replace($"@P{--i}", value);
            }
            return query;
        }

        public SerializableQuery SerializeSelect()
            => Serialize(QueryStr);                                
        public SerializableQuery SerializeUpdate()
            => Serialize(UpdateStr);        
        public SerializableQuery SerializeDelete()
            => Serialize(DeleteStr);
        private SerializableQuery Serialize(string query)
        => SerializableQuery.Get(query, Parameters);
        private DeserializedQuery Deserialize(SerializableQuery serializableQuery)
        => DeserializedQuery.Get(serializableQuery);        

        #endregion

        #region Private methods
        internal string AddUpdateProperty<T>(string field, object value)
        {
            var pIndex = ParameterIndex + 1;
            Parameters.Add($"P{++ParameterIndex}", value); //, dbType: PropertiesTypeCache.GetPropertyType(typeof(T), field));
            return $"{field} = @P{pIndex}";
        }       
        internal DapperFluentFilter FilterBase(string field, FilterOperator op, params object[] values)
        {
            var filter = new DapperFluentFilter();
            if (!field.Contains(".")) field = $"{TableName??FromClause}.{field}";           

            if (((values == null || values.Length == 0 || values[0] == null) && !(op == FilterOperator.IsNull || op == FilterOperator.NotNull)) &&
                ((values == null || string.IsNullOrEmpty(values[0]?.ToString()) && !(op == FilterOperator.Like || op == FilterOperator.NotLike)) ||
                (op == FilterOperator.Between && ((values == null || values.Length != 2) || (values == null || string.IsNullOrEmpty(values[1]?.ToString()))))))
                return filter;

            if (op == FilterOperator.In)
            {
                if (values == null) 
                    return filter;
                else if (!values.Any() ||
                    (values.ToList().First() is IList && (values.ToList().First() as IList).Count == 0) ||                    
                    (values.ToList().First() is Array && (values.ToList().First() as Array).Length == 0))
                    op = FilterOperator.IsNull;
            }

            if ((op == FilterOperator.BeginWith || op == FilterOperator.EndWith || 
                op == FilterOperator.NotBeginWith || op == FilterOperator.NotEndWith || 
                op == FilterOperator.LikeFull || op == FilterOperator.NotLikeFull)
                && string.IsNullOrEmpty(values[0].ToString()))
                return filter;

            var pIndex = ParameterIndex + 1;
            filter.CustomFilter = 
            (
                op == FilterOperator.In? $"{field} in @P{pIndex} ":
                op == FilterOperator.NotIn ? $"{field} not in @P{pIndex} " :
                op == FilterOperator.IsNull ? $"{field} is Null " :
                op == FilterOperator.NotNull ? $"{field} is not null " :
                op == FilterOperator.Distinct ? $"{field} <> @P{pIndex} " :
                op == FilterOperator.Equals ? $"{field} = @P{pIndex} " :
                op == FilterOperator.Like ? $"{field} like @P{pIndex} " :
                op == FilterOperator.LikeFull ? $"{field} like CONCAT('%', @P{pIndex}, '%') " :
                op == FilterOperator.EndWith ? $"{field} like CONCAT('%', @P{pIndex}) " :
                op == FilterOperator.BeginWith ? $"{field} like CONCAT(@P{pIndex}, '%') " :
                op == FilterOperator.NotLike ? $"{field} not like @P{pIndex} " :
                op == FilterOperator.NotLikeFull ? $"{field} not like CONCAT('%', @P{pIndex}, '%') " :
                op == FilterOperator.NotEndWith ? $"{field} not like CONCAT('%', @P{pIndex}) " :
                op == FilterOperator.NotBeginWith ? $"{field} not like CONCAT(@P{pIndex}, '%') " :
                op == FilterOperator.GreaterThan ? $"{field} > @P{pIndex} " :
                op == FilterOperator.GreaterThanOrEqual ? $"{field} >= @P{pIndex} " :
                op == FilterOperator.LesserThan ? $"{field} < @P{pIndex} " :
                op == FilterOperator.LessThanOrEqual ? $"{field} <= @P{pIndex} " :
                op == FilterOperator.Between ? $"{field} between @P{pIndex} and @P{++pIndex} " :
                string.Empty);
            var paramNumber = 
            (
                op == FilterOperator.In || op == FilterOperator.NotIn ? -1 :                
                op == FilterOperator.IsNull || op == FilterOperator.NotNull ? 0 :                
                op == FilterOperator.Between ? 2 : 1
            );
            if (paramNumber == -1)
                Parameters.Add($"P{++ParameterIndex}", values.ToList().First());

            for (int i = 0; i < paramNumber; i++)
                Parameters.Add($"P{++ParameterIndex}", values[i]);//, dbType: PropertiesTypeCache.GetPropertyType(modelType, field));
            
            return filter;
        }
        #endregion
    }
}
