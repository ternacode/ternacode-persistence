using System;

namespace Ternacode.Persistence.Example.API.Contracts.Errors
{
    public class ExceptionResponse
    {
        public ExceptionResponse(Exception exception)
        {
            ErrorMessage = exception == null ? "An error occurred" : exception.Message;
        }

        public string ErrorMessage { get; }
    }
}