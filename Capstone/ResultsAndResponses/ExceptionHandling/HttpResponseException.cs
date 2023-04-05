using System.Net;

namespace Capstone.Responses.ExceptionHandling
{
    public class HttpResponseException : Exception
    {
        public int StatusCode { get; }
        public object? Value { get; }

        public HttpResponseException(HttpStatusCode statusCode, object? value = null) =>
            (StatusCode, Value) = ((int)statusCode, value);
    }
}
