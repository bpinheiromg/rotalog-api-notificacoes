using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace api_notificacoes.Middleware;

/// <summary>
/// Middleware to extract or generate correlation ID for distributed tracing.
/// Reads X-Correlation-ID header from incoming requests, generates one if missing,
/// adds it to response headers, and stores in LogContext for structured logging.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    private readonly CorrelationIdOptions _options;

    public CorrelationIdMiddleware(
        RequestDelegate next,
        ILogger<CorrelationIdMiddleware> logger,
        IOptions<CorrelationIdOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[_options.HeaderName].FirstOrDefault();

        // Generate new correlation ID if not present
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
        }

        // Add to response header for downstream tracing
        context.Response.Headers[_options.HeaderName] = correlationId;

        // Add to LogContext for Serilog structured logging
        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        {
            _logger.LogInformation("Request started: {Method} {Path} | CorrelationId: {CorrelationId}",
                context.Request.Method, context.Request.Path, correlationId);

            try
            {
                await _next(context);
            }
            finally
            {
                _logger.LogInformation("Request completed: {Method} {Path} | Status: {StatusCode} | CorrelationId: {CorrelationId}",
                    context.Request.Method, context.Request.Path, context.Response.StatusCode, correlationId);
            }
        }
    }
}

/// <summary>
/// Configuration options for correlation ID middleware
/// </summary>
public class CorrelationIdOptions
{
    public const string SectionName = "CorrelationId";

    /// <summary>
    /// Header name for correlation ID (default: X-Correlation-ID)
    /// </summary>
    public string HeaderName { get; set; } = "X-Correlation-ID";
}