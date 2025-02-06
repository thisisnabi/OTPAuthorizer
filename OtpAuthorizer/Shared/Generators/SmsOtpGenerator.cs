using Microsoft.Extensions.Options;
namespace OtpAuthorizer.Shared.Generators;

public class SmsOtpGenerator(IOptions<AppSettings> options) 
    : BaseOtpGenerator(options.Value.OtpConfigurations.Channels.First(d => d.Name == OtpChannel.Sms))
{

    public override GenerateOtpDto Generate(string client)
    {
        // TODO: Add client validator for phone number format!

        var expireOnUtc = DateTime.UtcNow.AddMinutes(_configurations.ExpireOnMinutes);
        var code = GeneratorCode(_configurations.Length, _configurations.MaxRepeatNumber);

        var message = $"""
                       رمز یک بار مصرف شما عبارت است از:
                       {code}

                       لغو  11
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
