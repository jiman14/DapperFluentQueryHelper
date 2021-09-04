using System;
using System.Data;
using System.Linq.Expressions;

namespace DapperFluentQueryHelper.Core
{
    public interface IDUpdate
    {
        bool Update<T>(IDbConnection connection);

        IDUpdate Update<T>(Expression<Func<T>> expression);
    }    
}
