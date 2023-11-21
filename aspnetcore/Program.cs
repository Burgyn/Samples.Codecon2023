using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

#region Slim and Empty builder

// 👇 AOT
//var builder = WebApplication.CreateEmptyBuilder(args);
//var builder = WebApplication.CreateSlimBuilder(args);

// 🚫 Limitations in the Current Implementation:
// ❌ No support for startup assemblies (IHostingStartup)
// ❌ No support for calling UseStartup<Startup>
// ❌ Fewer logging providers
//   - ❌ No EventLog log provider for logging to the Windows event log
//   - ❌ No Debug provider for logging to the debugger console
//   - ❌ No EventSource provider for writing to ETW (Windows) or LTTng (Linux)
// ❌ Missing web hosting features
//   - ❌ No support for UseStaticWebAssets() for loading static assets from referenced projects and packages
//   - ❌ No IIS integration
// ❌ Missing features in the Kestrel configuration
//   - ❌ No HTTPS support
//   - ❌ No Quic (HTTP/3) support
// ❌ No support for Regex or alpha constraints in routing

#endregion

#region Swagger
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
#endregion

#region Keyed servies

builder.Services
    .AddKeyedSingleton<ILoyaltySystemService, BasicLoyaltySystemService>("basic");
builder.Services
    .AddKeyedSingleton<ILoyaltySystemService, GoldLoyaltySystemService>("gold");

#endregion

#region Request timeout

//builder.Services.AddRequestTimeouts();
builder.Services.AddRequestTimeouts(options =>
{
    options.DefaultPolicy = new RequestTimeoutPolicy
    {
        Timeout = TimeSpan.FromMilliseconds(1000),
        TimeoutStatusCode = 503
    };
    options.AddPolicy("MyPolicy2", new RequestTimeoutPolicy
    {
        Timeout = TimeSpan.FromMilliseconds(1000),
        WriteTimeoutResponse = async (HttpContext context) =>
        {
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync("Timeout from MyPolicy2!");
        }
    });
});

#endregion

#region IExceptionHandler

builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<DefaultExceptionHandler>();

#endregion

#region Identity Endpoints

// Add authentication
builder.Services
    .AddAuthentication()
    .AddBearerToken(IdentityConstants.BearerScheme);

// Add Authorization
builder.Services.AddAuthorizationBuilder();

// Add DbContext
builder.Services
    .AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("AppDb"));

// Map Identity 👇
builder.Services
    .AddIdentityCore<MyUser>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddApiEndpoints();

#endregion

var app = builder.Build();

#region ShortCircuit

app.MapGet("/robots.txt",
    () => """
          User-agent: *
          Allow: /
          """).ShortCircuit(200); // 👈 return a valid robots.txt

app.MapGet("/health", () => "OK") // 👈 return a valid health check response
    .ShortCircuit(200);

app.MapShortCircuit(404, ".well-known", "/favicon.ico"); // 👈 return a valid 404 for favicon.ico and .well-known

#endregion

#region Request timeout

app.UseRequestTimeouts();

app.MapGet("/timeout", async (HttpContext context) =>
{
    await Task.Delay(TimeSpan.FromSeconds(10), context.RequestAborted);

    return Results.Ok("Hello CODECON!");
}).WithRequestTimeout(TimeSpan.FromSeconds(2));

app.MapGet("/timeout/namedpolicy", async (HttpContext context) =>
{
    await Task.Delay(TimeSpan.FromSeconds(10), context.RequestAborted);

    return Results.Ok("Hello CODECON!");
}).WithRequestTimeout("MyPolicy");

app.MapGet("/timeout/attribute", [RequestTimeout(milliseconds: 2000)] async (HttpContext context) =>
{
    await Task.Delay(TimeSpan.FromSeconds(10), context.RequestAborted);

    return Results.Ok("Hello CODECON!");
});

app.MapGet("/timeout/disabled", async (HttpContext context) =>
{
    await Task.Delay(TimeSpan.FromSeconds(10), context.RequestAborted);

    return Results.Ok("Hello CODECON!");
}).DisableRequestTimeout();

app.MapGet("/timeout/canceltimeout", async (HttpContext context) =>
{
    var timeoutFeature = context.Features.Get<IHttpRequestTimeoutFeature>();
    timeoutFeature?.DisableTimeout();

    await Task.Delay(TimeSpan.FromSeconds(10), context.RequestAborted);
    return Results.Ok("Hello CODECON!");
});

#endregion

#region From form

app.MapPost("/handle-form", ([FromForm] string firstName, [FromForm] string lastName)
    => Results.Ok($"{firstName} {lastName}"))
    .DisableAntiforgery();

#endregion

#region Keyed services

app.MapGet("/customer/discount",
    ([FromKeyedServices("basic")] ILoyaltySystemService loyaltySystemService)
    => loyaltySystemService.GetDiscount());

app.MapPost("/customer/discount2",
    (IServiceProvider serviceProvider, AppUser identity)
    => serviceProvider.GetRequiredKeyedService<ILoyaltySystemService>(identity.Type).GetDiscount());

#endregion

#region Swagger
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
#endregion

#region IExceptionHandler

app.UseExceptionHandler(_ => { });
app.MapGet("/exception", () =>
{
    throw new NotFoundException();
});

#endregion

#region Identity Endpoints

app.MapGroup("/account")
    .MapIdentityApi<MyUser>();

#endregion

app.Run();

#region Keyed services

public interface ILoyaltySystemService
{
    decimal GetDiscount();
}

public class BasicLoyaltySystemService : ILoyaltySystemService
{
    public decimal GetDiscount() => 0.1m;
}

public class GoldLoyaltySystemService : ILoyaltySystemService
{
    public decimal GetDiscount() => 0.25m;
}

#endregion

#region IExpcetionHandler

public class NotFoundExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is NotFoundException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await httpContext.Response.WriteAsync("Resource not found", cancellationToken);

            return true;
        }

        return false;
    }
}

public class DefaultExceptionHandler : IExceptionHandler
{
    private readonly ILogger<DefaultExceptionHandler> _logger;
    public DefaultExceptionHandler(ILogger<DefaultExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unexpected error occurred");

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = (int)HttpStatusCode.InternalServerError,
            Type = exception.GetType().Name,
            Title = "An unexpected error occurred",
            Detail = exception.Message,
            Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
        }, cancellationToken: cancellationToken);

        return true;
    }
}

#endregion
