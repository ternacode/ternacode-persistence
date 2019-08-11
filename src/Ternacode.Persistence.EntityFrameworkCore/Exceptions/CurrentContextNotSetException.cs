using System;

namespace Ternacode.Persistence.EntityFrameworkCore.Exceptions
{
    public class CurrentContextNotSetException : Exception
    {
        public CurrentContextNotSetException()
            : base("No current context set")
        {
        }
    }
}