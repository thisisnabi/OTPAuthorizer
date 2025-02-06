namespace OtpAuthorizer.Shared.Generators;


public interface IOtpGeneratorDecorator
{
    public GenerateOtpDto Generate(string client);
}

public class OtpGeneratorDecorator(IOtpGeneratorDecorator service) : IOtpGeneratorDecorator
{
    private readonly IOtpGeneratorDecorator _service = service;

    public GenerateOtpDto Generate(string client)
       => _service.Generate(client);
}
 
public record GenerateOtpDto(
    string Code,
    string Message,
    string Client, 
    int Length, 
    int ExpireOnMinutes, 
    DateTime ExpirationDateTimeUtc);