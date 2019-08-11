using System;

namespace Ternacode.Persistence.EntityFrameworkCore.Exceptions
{
    public class CurrentContextAlreadySetException : Exception
    {
        public CurrentContextAlreadySetException()
            : base("Current context already set")
        {
        }
    }
}