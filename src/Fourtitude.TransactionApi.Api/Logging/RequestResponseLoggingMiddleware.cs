using System.Diagnostics;
namespace Fourtitude.TransactionApi.Api.Logging
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
        private readonly SensitiveDataProtector _protector;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger, SensitiveDataProtector protector)
        {
            _next = next;
            _logger = logger;
            _protector = protector;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            context.Request.EnableBuffering();
            var requestBody = await new StreamReader(context.Request.Body, leaveOpen: true).ReadToEndAsync();
            context.Request.Body.Position = 0;

            _logger.LogInformation(
                "Request {Method} {Path} Body : {Body}",
                context.Request.Method,
                context.Request.Path,
                _protector.Protect(requestBody));

            var originalResponseBody = context.Response.Body;
            using var responseBuffer = new MemoryStream();
            context.Response.Body = responseBuffer;

            try
            {
                await _next(context);

            }
            finally
            {
                responseBuffer.Position = 0;
                var responseBody = await new
                    StreamReader(responseBuffer).ReadToEndAsync();

                _logger.LogInformation(
                    "Response {StatusCode} in {ElapsedMs} ms Body: {Body}",
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    responseBody);

                responseBuffer.Position = 0;
                await responseBuffer.CopyToAsync(originalResponseBody);
                context.Response.Body = originalResponseBody;
            }
        }
    }
}
