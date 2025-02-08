using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using OtpAuthorizer.Endpoints;
using OtpAuthorizer.Shared;
using OtpAuthorizer.Shared.Generators;
using Scalar.AspNetCore;
using StackExchange.Redis;
using System.IO.Pipes;
using System.Security.Claims;
using System.Text.Encodings.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.Configure<AppSettings>(builder.Configuration);
builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(
        builder.Configuration.GetConnectionString("Redis")
            ?? throw new NullReferenceException("Redis")));

builder.Services.AddScoped<SharedContext>();


builder.Services.AddScoped<IOtpGeneratorDecorator>(sp =>
{
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    var httpContext = httpContextAccessor.HttpContext
                        ?? throw new NullReferenceException(nameof(httpContextAccessor.HttpContext));

    if (httpContext.Request.Path.StartsWithSegments("/otp/email"))
        return new OtpGeneratorDecorator(ActivatorUtilities.CreateInstance<EmailOtpGenerator>(sp));

    if (httpContext.Request.Path.StartsWithSegments("/otp/sms"))
        return new OtpGeneratorDecorator(ActivatorUtilities.CreateInstance<SmsOtpGenerator>(sp));

    throw new InvalidCastException();
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "MultiAuth";
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.Authority = "idp.thisisnabi.dev";
    options.Audience = "otp.thisisnabi.dev";
}).AddScheme<AuthenticationSchemeOptions, BusinessIdAuthenticationHandler>("X_Business_Id", null);


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BusinessId", policy => policy.RequireAuthenticatedUser());
    options.AddPolicy("QA_Claim", policy => policy.RequireAuthenticatedUser().RequireClaim("QA_Access"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}


app.UseAuthentication();
app.UseAuthorization();

app.MapGroup("otp/{channel}/{client}/")
   .MapOtpGeneratorEndpoint()
   .MapOtpVerifyEndpoint()
   .RequireAuthorization("BusinessId");
 
app.MapOtpCatchEndpoint();


// check qa access
app.MapGet("otp/{channel}/{client}/catch", () =>
{


});

app.Run();
 

public sealed class SharedContext(IConnectionMultiplexer multiplexer)
{
    public const string OtpKeyPattern = "otp:authorizer:{0}:{1}";

    public IDatabase RedisDatabase => multiplexer.GetDatabase();
}

public class BusinessIdAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private static List<string> _businessIds = 
        [
            "credit",
            "bnpl", 
            "wealth", 
            "trip" 
        ];

    private const string header_name = "business_id";

    public BusinessIdAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
    {

    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if(!Request.Headers.TryGetValue(header_name, out var businessId) ||
           !_businessIds.Contains(businessId.ToString()))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid business Id"));
        }

        var claims = new[] { new Claim("business-id", businessId.ToString()) };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}