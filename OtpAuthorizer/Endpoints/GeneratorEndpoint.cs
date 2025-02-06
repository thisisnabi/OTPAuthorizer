using Microsoft.AspNetCore.Mvc;
using OtpAuthorizer.Shared.Generators;

namespace OtpAuthorizer.Endpoints;

public static class GeneratorEndpoint
{
    public static RouteGroupBuilder MapOtpGeneratorEndpoint(this RouteGroupBuilder routeGroup)
    {
        routeGroup.MapPost("generate", 
            async (IOtpGeneratorDecorator otpGenerator, SharedContext sharedContext,
            [FromRoute] string channel, [FromRoute] string client) =>
            {
                var otpGeneratorResult = otpGenerator.Generate(client);

                //you can encrypt your OTP [otpGeneratorResult.Code] 
                var expiry = TimeSpan.FromMinutes(otpGeneratorResult.ExpireOnMinutes);
                var otpKey = string.Format(SharedContext.OtpKeyPattern, channel, client);
                await sharedContext.RedisDatabase.StringSetAsync(otpKey, otpGeneratorResult.Code, expiry);
                  
                // push (message + channel + client) to notiy service
                // otpGeneratorResult.Message
                // otpGeneratorResult.Client
                // Channel

                return Results.Ok(new OtpGeneratorResponse(
                    otpGeneratorResult.Client,
                    otpGeneratorResult.Length,
                    otpGeneratorResult.ExpireOnMinutes,
                    otpGeneratorResult.ExpirationDateTimeUtc));
            });

        return routeGroup;
    }
}

public record OtpGeneratorResponse(string Client,
    int Length,
    int ExpireOnMinutes,
    DateTime ExpirationDateTimeUtc);
