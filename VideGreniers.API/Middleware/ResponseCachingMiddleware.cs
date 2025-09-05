namespace VideGreniers.API.Middleware;

/// <summary>
/// Middleware for adding response caching headers based on endpoint patterns
/// </summary>
public class ResponseCachingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ResponseCachingMiddleware> _logger;

    public ResponseCachingMiddleware(RequestDelegate next, ILogger<ResponseCachingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        // Only apply caching to successful GET requests
        if (context.Request.Method == HttpMethods.Get && 
            context.Response.StatusCode == StatusCodes.Status200OK)
        {
            ApplyCachingHeaders(context);
        }
    }

    private void ApplyCachingHeaders(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant();
        
        if (path == null) return;

        // Skip caching if headers are already set
        if (context.Response.Headers.ContainsKey("Cache-Control")) return;

        var cacheSettings = GetCacheSettingsForPath(path);
        if (cacheSettings != null)
        {
            context.Response.Headers.CacheControl = cacheSettings.CacheControl;
            
            if (!string.IsNullOrEmpty(cacheSettings.ETag))
            {
                context.Response.Headers.ETag = cacheSettings.ETag;
            }

            if (cacheSettings.LastModified.HasValue)
            {
                context.Response.Headers.LastModified = cacheSettings.LastModified.Value.ToString("R");
            }

            // Add Vary header for API responses that might vary by Accept or Authorization
            if (path.StartsWith("/api/"))
            {
                context.Response.Headers.Vary = "Accept,Accept-Encoding,Authorization";
            }

            _logger.LogDebug("Applied caching headers for {Path}: {CacheControl}", 
                path, cacheSettings.CacheControl);
        }
    }

    private CacheSettings? GetCacheSettingsForPath(string path)
    {
        return path switch
        {
            // Events endpoints
            var p when p.StartsWith("/api/events") && !p.Contains("nearby") => new CacheSettings
            {
                CacheControl = "public, max-age=300", // 5 minutes
                ETag = GenerateETag(path)
            },
            
            // Nearby events (shorter cache due to location dependency)
            var p when p.Contains("/api/events/nearby") => new CacheSettings
            {
                CacheControl = "public, max-age=60" // 1 minute
            },

            // Health endpoints
            var p when p.StartsWith("/health") => new CacheSettings
            {
                CacheControl = "no-cache, no-store, must-revalidate"
            },

            // Static files
            var p when p.Contains(".js") || p.Contains(".css") || p.Contains(".ico") => new CacheSettings
            {
                CacheControl = "public, max-age=86400", // 24 hours
                LastModified = DateTime.UtcNow.AddHours(-1)
            },

            // Images and media
            var p when p.Contains(".jpg") || p.Contains(".png") || p.Contains(".gif") || p.Contains(".webp") => new CacheSettings
            {
                CacheControl = "public, max-age=604800" // 7 days
            },

            // Swagger/OpenAPI documentation
            var p when p.Contains("/swagger") || p.Contains("/openapi") => new CacheSettings
            {
                CacheControl = "public, max-age=3600" // 1 hour
            },

            _ => null
        };
    }

    private string GenerateETag(string path)
    {
        // Generate a simple ETag based on path and current hour
        // In production, this should be based on actual content hash or version
        var content = $"{path}-{DateTime.UtcNow:yyyyMMddHH}";
        var hash = content.GetHashCode();
        return $"\"{hash:X}\"";
    }

    private class CacheSettings
    {
        public string CacheControl { get; set; } = string.Empty;
        public string? ETag { get; set; }
        public DateTime? LastModified { get; set; }
    }
}