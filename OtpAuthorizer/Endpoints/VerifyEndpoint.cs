using Microsoft.AspNetCore.Mvc;
using OtpAuthorizer.Shared.Generators;

namespace OtpAuthorizer.Endpoints;

public static class VerifyEndpoint
{
    public static RouteGroupBuilder MapOtpVerifyEndpoint(this RouteGroupBuilder routeGroup)
    {
        routeGroup.MapGet("verify", 
            async (SharedContext sharedContext,
            [FromRoute] string channel, [FromRoute] string client, 
            [FromQuery(Name = "otp_code")]string otpCode) =>
            {
                //you can encrypt your OTP [otpGeneratorResult.Code] 
                var otpKey = string.Format(SharedContext.OtpKeyPattern, channel, client);
                var storedOtp = await sharedContext.RedisDatabase.StringGetAsync(otpKey);
                  
                if(otpCode == storedOtp)
                {
                    await sharedContext.RedisDatabase.KeyDeleteAsync(otpKey);
                    return Results.Ok("OTP verified.");
                }

                return Results.BadRequest("Invalid OTP.");
            });

        return routeGroup;
    }
}