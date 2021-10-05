using System;
using System.Collections.Concurrent;
using System.Data;
using System.Linq;

namespace DapperFluentQueryHelper.Core
{
    #region Filter helpers
    public class DapperFluentFilters
    {
        public string FiltersStr { get; set; }
        public DapperFluentFilters(string filters) => FiltersStr = filters;
    }
    public class DapperFluentFilter
    {
        public string CustomFilter { get; set; }
    }
    public class DapperFluentJoinFilter
    {
        private string LeftJoinField { get; set; }
        private JoinOperator Operator { get; set; }
        private string RightJoinField { get; set; }
        
        public DapperFluentJoinFilter(string leftJoinField, JoinOperator joinOperator, string rightJoinField)
        {
            LeftJoinField = leftJoinField;
            Operator = joinOperator;
            RightJoinField = rightJoinField;
        }
        public string GetJoin(JoinType joinType) 
        => $" {joinType.ToString()} JOIN {RightJoinField.Split('.').First()}";

        public string GetJoinFilter()
        => $"{LeftJoinField} " + 
            (Operator == JoinOperator.Distinct? "!=":
            Operator == JoinOperator.Mayor? ">":
            Operator == JoinOperator.Minor? "<": "=") + 
            $"{RightJoinField}";
    }
    #endregion

    #region Enums
    public enum JoinOperator
    {
        Distinct,
        Equals,
        Mayor,
        Minor
    }
    public enum JoinType
    {
        Inner,
        Left,
        Right
    }
    public enum QueryOrderBy
    {
        asc,
        desc
    }
    public enum SortDirection
    {
        Ascending,
        Descending
    }
    public enum FilterOperator
    {
        Between,
        Distinct,
        Equals,
        In,
        IsNull,
        Like,
        LikeFull,
        EndWith,
        BeginWith,
        GreaterThan,
        GreaterThanOrEqual,
        LesserThan,
        LessThanOrEqual,
        NotIn,
        NotLike,
        NotLikeFull,
        NotNull,
        NotEndWith,
        NotBeginWith
    }
    #endregion

    #region Properties type cache

    public static class PropertiesTypeCache
    {
        private static ConcurrentDictionary<string, Type> TypesCache = new ConcurrentDictionary<string, Type>();

        public static DbType GetPropertyType(Type modelType, string propertyName) 
            => NetTypesToDBConversions.GetDBType(GetPropertyInfo(modelType, propertyName));
        public static void SetPropertyType(string propertyName, Type propertyType) 
            => TypesCache.TryAdd(propertyName, propertyType);
        private static Type GetPropertyInfo(Type modelType, string propertyName)
        {
            if (!TypesCache.TryGetValue(propertyName, out Type value))
            {
                value = modelType.GetProperty(propertyName.Split('.').Last()).PropertyType;
                TypesCache.TryAdd(propertyName, value);
            }
            return value;
        }
    }
    #endregion

    #region Net to SQL type conversion
    public static class NetTypesToDBConversions
    {
        /// <summary>
        /// Net types array
        /// </summary>
        public static string[] NetTypes = new string[] { "Byte", "byte[]", "bool", "char", "DateTime", "decimal", "double", "int", "Int16", "long", "single", "string" };

        internal static DbType GetDBType(string propType)
        => GetDBType(Type.GetType(propType));
                
        internal static DbType GetDBType(Type propType)
        {
            if (propType == typeof(string))
                return DbType.String;
            else if (propType == typeof(int) || propType == typeof(Int32) || propType == typeof(Nullable<int>) || propType == typeof(Nullable<Int32>))
                return DbType.Int32;
            else if (propType == typeof(bool) || propType == typeof(Nullable<bool>))
                return DbType.Boolean;
            else if (propType == typeof(TimeSpan) || propType == typeof(Nullable<TimeSpan>))
                return DbType.Int64;
            else if (propType == typeof(DateTime) || propType == typeof(Nullable<DateTime>))
                return DbType.DateTime;
            else if (propType == typeof(decimal) || propType == typeof(Nullable<decimal>))
                return DbType.Currency;
            else if (propType == typeof(float) || propType == typeof(double) || propType == typeof(Nullable<double>))
                return DbType.Double;
            else if (propType == typeof(byte[]))
                return DbType.Binary;
            else if (propType == typeof(long) || propType == typeof(Int64) || propType == typeof(Nullable<long>) || propType == typeof(Nullable<Int64>))
                return DbType.Int64;
            else if (propType == typeof(char) || propType == typeof(Nullable<char>))
                return DbType.Byte;
            else if (propType == typeof(Int16) || propType == typeof(Nullable<Int16>))
                return DbType.Int16;
            else
                throw new Exception($"{nameof(NetTypesToDBConversions)}. DB Conversion not implemented");
        }
    }
    #endregion
}
