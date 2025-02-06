using Microsoft.AspNetCore.Mvc;
using OtpAuthorizer.Shared.Generators;

namespace OtpAuthorizer.Endpoints;

public static class CatchEndpoint
{
    public static IEndpointRouteBuilder MapOtpCatchEndpoint(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapPost("otp/{channel}/{client}/catch", 
            async (SharedContext sharedContext,
            [FromRoute] string channel, [FromRoute] string client) =>
            {
                //you can encrypt your OTP [otpGeneratorResult.Code] 
                var otpKey = string.Format(SharedContext.OtpKeyPattern, channel, client);
                var storedOtp = await sharedContext.RedisDatabase.StringGetAsync(otpKey);
                  
                if(string.IsNullOrEmpty(storedOtp))
                {
                    await sharedContext.RedisDatabase.KeyDeleteAsync(otpKey);
                    return Results.Ok(null);
                }
                 
                return Results.Ok(new
                {
                    Otp = storedOtp,
                    Client = client
                });
            }).RequireAuthorization("QA_Claim");

        return routeBuilder;
    }
}