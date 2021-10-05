using Dapper;
using System;
using System.Linq;

namespace DapperFluentQueryHelper.Core.Serializer
{
    [Serializable]
    public class SerializableParameter
    {
        public string Name { get; set; }
        public string ParamType { get; set; }
        public string Value { get; set; }
        public SerializableParameter(string name, string paramType, string value)
        {
            Name = name;
            ParamType = paramType;
            Value = value;
        }
    }
    [Serializable]
    public class SerializableQuery
    {
        public string Query { get; set; }
        public SerializableParameter[] Parameters { get; set; }
        public static SerializableQuery Get(string query, DynamicParameters parameters)
        => new SerializableQuery
        {
            Query = query,
            Parameters = SerializeParamaters.GetList(parameters).ToArray()
        };
    }
    public class DeserializedQuery
    {
        public string Query { get; set; }
        public DynamicParameters Parameters { get; set; } = new DynamicParameters();
        public static DeserializedQuery Get(SerializableQuery serializableQuery)
        {
            var deserializedQuery = new DeserializedQuery { Query = serializableQuery.Query };
            serializableQuery.Parameters.ToList().ForEach(
                p => deserializedQuery.Parameters.Add(p.Name, Convert.ChangeType(p.Value, Type.GetType(p.ParamType)) , NetTypesToDBConversions.GetDBType(p.ParamType)));
            return deserializedQuery;
        }
    }
}
