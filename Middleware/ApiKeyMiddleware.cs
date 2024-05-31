using Microsoft.Extensions.Options;

namespace WeatherAPI.Middleware
{
    public class ApiKeyMiddlewareOptions
    {
        public string ApiKey { get; set; }
    }

    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IOptions<ApiKeyMiddlewareOptions> _options;

        public ApiKeyMiddleware(RequestDelegate next, IOptions<ApiKeyMiddlewareOptions> options)
        {
            _next = next;
            _options = options;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            const string ApiKeyHeaderName = "ApiKey";

            if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API Key was not provided.");
                return;
            }

            if (!_options.Value.ApiKey.Equals(extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized client.");
                return;
            }

            await _next(context);
        }
    }
}
