using System;
using System.Collections.Concurrent;
using System.Data;
using System.Linq;

namespace LowCodingSoftware.FluentQuery
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
        => $" {joinType.ToString()} JOIN {RightJoinField.Split(".").First()}";

        public string GetJoinFilter()
        => $"{LeftJoinField} " + 
            Operator switch {
                JoinOperator.Distinct => "!=",
                JoinOperator.Mayor => ">",
                JoinOperator.Minor => "<",
                _ => "=" } + 
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
        GreaterOrEqualsThan,
        LesserThan,
        LesserOrEqualsThan,
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
        
        private static Type GetPropertyInfo(Type modelType, string propertyName)
        {
            if (!TypesCache.TryGetValue(propertyName, out Type value))
            {
                value = modelType.GetProperty(propertyName.Split(".").Last()).PropertyType;
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

        internal static DbType GetDBType(Type PropType)
        {
            if (PropType == typeof(string))
                return DbType.String;
            else if (PropType == typeof(int) || PropType == typeof(Int32) || PropType == typeof(Nullable<int>) || PropType == typeof(Nullable<Int32>))
                return DbType.Int32;
            else if (PropType == typeof(bool) || PropType == typeof(Nullable<bool>))
                return DbType.Boolean;
            else if (PropType == typeof(TimeSpan) || PropType == typeof(Nullable<TimeSpan>))
                return DbType.Int64;
            else if (PropType == typeof(DateTime) || PropType == typeof(Nullable<DateTime>))
                return DbType.DateTime;
            else if (PropType == typeof(decimal) || PropType == typeof(Nullable<decimal>))
                return DbType.Currency;
            else if (PropType == typeof(float) || PropType == typeof(double) || PropType == typeof(Nullable<double>))
                return DbType.Double;
            else if (PropType == typeof(byte[]))
                return DbType.Binary;
            else if (PropType == typeof(long) || PropType == typeof(Int64) || PropType == typeof(Nullable<long>) || PropType == typeof(Nullable<Int64>))
                return DbType.Int64;
            else if (PropType == typeof(char) || PropType == typeof(Nullable<char>))
                return DbType.Byte;
            else if (PropType == typeof(Int16) || PropType == typeof(Nullable<Int16>))
                return DbType.Int16;
            else
                throw new Exception($"{nameof(NetTypesToDBConversions)}. DB Conversion not implemented");
        }
    }
    #endregion
}
