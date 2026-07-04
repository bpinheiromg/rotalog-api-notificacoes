using Serilog.Context;

namespace api_notificacoes.Extensions;

/// <summary>
/// Helper extensions for Serilog LogContext
/// </summary>
public static class LogContextExtensions
{
    /// <summary>
    /// Gets the current correlation ID from LogContext
    /// </summary>
    public static string? GetCurrentCorrelationId()
    {
        // LogContext.GetProperty is available in newer Serilog versions
        // For now, we'll use reflection or return null as fallback
        try
        {
            // Try to get property from LogContext using the internal API
            var property = typeof(LogContext).GetProperty("CorrelationId",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (property != null)
            {
                return property.GetValue(null) as string;
            }
        }
        catch
        {
            // Ignore reflection errors
        }
        return null;
    }
}