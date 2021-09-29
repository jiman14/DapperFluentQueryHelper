using DapperFluentQueryHelper.Core;
using System;

namespace Example
{
    internal class DapperFluentQueryExample
    {
        public DapperFluentQueryExample()
        {
            new DUpdate().Update(() => new ClaseEjemplo { MyProperty1 = 1, MyProperty2 = "asdf" });                  
        }
    }

    public class ClaseEjemplo
    {
        public int MyProperty1 { get; set; }
        public string MyProperty2 { get; set; }
        public string MyProperty3 { get; set; }
    }
}
