using System;

namespace MySQLConnection.Core
{
    public class SQLException: Exception
    {
        public SQLException(string message, Exception innerException) : base(message, innerException) { }
        public SQLException(Exception innerException) : base(nameof(MySQLConnection), innerException) { }
    }
}
