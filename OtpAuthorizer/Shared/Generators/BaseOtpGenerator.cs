namespace OtpAuthorizer.Shared.Generators;

public abstract class BaseOtpGenerator(OptChannelConfiguration optChannelConfiguration) 
    : IOtpGeneratorDecorator
{
    protected readonly OptChannelConfiguration _configurations = optChannelConfiguration;

    public abstract GenerateOtpDto Generate(string client);
     
    protected string GeneratorCode(int length, int maxRepeatNumber)
    {
        var digits = new List<int>(length);

        for (int index = 0; index < length; index++)
        {
            int digit;

            do
            {
                digit = new Random().Next((_configurations.StartWithZero || index != 0 ? 0 : 1), 9);
            }
            while (digits.Any(d => d == digit) &&
                    digits.Where(d => d == digit).Count() >= maxRepeatNumber);

            digits.Add(digit);
        }

        return string.Join("", digits);
    }
}
