using System;

namespace DapperFluentQueryHelper.Core
{
    [Serializable]
    public abstract class BaseModel : ICloneable
    {
        public object Clone() => this.MemberwiseClone();        
    }
}