using DapperFluentQueryHelper.Core;

namespace Example
{
    internal class DapperFluentQueryExample
    {
        public DapperFluentQueryExample()
        {
            DUpdate update = new DUpdate();
            
            update.Update(() => new ClaseEjemplo { MyProperty1 = 1, MyProperty2 = "asdf" });
            var ce = new ClaseEjemplo { MyProperty1 = 1};
            update.Filter(() => ce.MyProperty1 != 2);
        }
    }

    public class ClaseEjemplo
    {
        public int MyProperty1 { get; set; }
        public string MyProperty2 { get; set; }
    }
}
