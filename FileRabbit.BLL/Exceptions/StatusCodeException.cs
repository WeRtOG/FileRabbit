using System;

namespace FileRabbit.BLL.Exceptions
{
    public class StatusCodeException : Exception
    {
        public StatusCodeException(string message, int statusCode) : base(message)
        {
            Data["Status code"] = statusCode;
        }
    }
}
