using System;

namespace DapperFluentQueryHelper.Core
{
    public abstract class BaseModel : ICloneable
    {
        public object Clone() => this.MemberwiseClone();        
    }
}