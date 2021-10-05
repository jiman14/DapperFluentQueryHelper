using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DapperFluentQueryHelper.Core.Serializer
{
    public static class SerializeParamaters
    {
        public static List<SerializableParameter> GetList(this DynamicParameters dynamicParams)
        {
            var paramList = new List<SerializableParameter>();
            var iLookup = (SqlMapper.IParameterLookup)dynamicParams;
            
            foreach (var paramName in dynamicParams.ParameterNames)
            {
                var value = iLookup[paramName];
                paramList.Add(new SerializableParameter(paramName, value.GetType().FullName, value.ToString()));
            }            
            // read the "templates" field containing dynamic parameters section added
            // via dynamicParams.Add(new {PARAM_1 = value1, PARAM_2 = value2});
            var templates = dynamicParams.GetType().GetField("templates", BindingFlags.NonPublic | BindingFlags.Instance);
            if (templates != null)
            {
                var list = templates.GetValue(dynamicParams) as List<Object>;
                if (list != null)
                {
                    // add properties of each dynamic parameters section
                    foreach (var objProps in list.Select(obj => obj.GetPropertyValuePairs().ToList()))
                        objProps.ForEach(p => paramList.Add(p));                    
                }
            }
            return paramList;
        }
        private static IEnumerable<SerializableParameter> GetPropertyValuePairs(this object obj, String[] hidden = null)
        {
            var type = obj.GetType();
            var pairs = hidden == null
                ? type.GetProperties()
                    .DistinctBy(propertyInfo => propertyInfo.Name)
                    .Select(propertyInfo => new SerializableParameter(propertyInfo.Name, propertyInfo.PropertyType.FullName, propertyInfo.GetValue(obj, null).ToString()))
                : type.GetProperties()
                    .Where(it => !hidden.Contains(it.Name))
                    .DistinctBy(propertyInfo => propertyInfo.Name)
                    .Select(propertyInfo => new SerializableParameter(propertyInfo.Name, propertyInfo.PropertyType.FullName, propertyInfo.GetValue(obj, null).ToString()));                        
            return pairs;
        }

        private static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            return source.Where(element => seenKeys.Add(keySelector(element)));
        }   
    }
}
