
using Microsoft.Extensions.Options;

namespace OtpAuthorizer.Shared.Generators;

public class EmailOtpGenerator(IOptions<AppSettings> options) :
    BaseOtpGenerator(options.Value.OtpConfigurations.Channels.First(d => d.Name == OtpChannel.Email))
{
     
    public override GenerateOtpDto Generate(string client)
    {
        // TODO: Add client validator for email format!

        var expireOnUtc = DateTime.UtcNow.AddMinutes(_configurations.ExpireOnMinutes);
        var code = GeneratorCode(_configurations.Length, _configurations.MaxRepeatNumber);

        var message = $"""
                       <html>
                            <head>
                                <title>Your site title</title>
                            <head>
                            <body>
                                <p>Your verification code is: {code}</p>
                            </body>
                       </html>
                       """;
        return new GenerateOtpDto(
            code,
            message,
            client,
            _configurations.Length,
            _configurations.ExpireOnMinutes,
            expireOnUtc);
    }

}