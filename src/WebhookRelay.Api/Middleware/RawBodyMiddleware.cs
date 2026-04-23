namespace WebhookRelay.Api.Middleware;

public class RawBodyMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.EnableBuffering();
        using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;
        context.Items["RawBody"] = rawBody;
        await next(context);
    }
}
