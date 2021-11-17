using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DapperFluentQueryHelper.Core
{
    public class DUpdate : DFilteredQuery
    {
        public DUpdate Update<T>(Expression<Func<T>> expression)
        {
            UpdateFields = string.Join(", ", ((MemberInitExpression)expression?.Body)?.Bindings.Select(p =>
                AddUpdateProperty<T>(GetField(p.Member), GetValue(((MemberAssignment)p).Expression, null))
            ));
            CommandType = CommandTypes.update;
            var modelType = typeof(T);
            TableName = modelType.Name.Replace("DB_", "");
            ModelTypes.TryAdd(modelType.Name, modelType);
            return this;
        }
        private string GetField(MemberInfo member)
            => $"{member.DeclaringType.Name.Replace("DB_", "")}.{member.Name}";
        private object GetValue(Expression expression, MemberExpression memberExp)
        {
            object ret = null;
            if (expression is MemberExpression)
                ret = GetValue((expression as MemberExpression).Expression, expression as MemberExpression);
            if (expression is ConstantExpression)
            {
                var value = (expression as ConstantExpression).Value;
                var valueType = value.GetType();
                ret = (valueType.IsNested)?                
                    value = valueType.InvokeMember(memberExp.Member.Name, BindingFlags.GetField, null, value, null):
                    (expression as ConstantExpression).Value;                
            }
            return ret;
        }        

        public DUpdate Where(Func<DFilteredQuery, DapperFluentFilter> where)
        {
            var filter = where.Invoke(this);
            return Where(filter.CustomFilter);
        }
        private DUpdate Where(string filtersStr)
        {
            WhereClause = string.IsNullOrEmpty(filtersStr) ? string.Empty : $"WHERE {filtersStr}";
            return this;
        }
    }
}