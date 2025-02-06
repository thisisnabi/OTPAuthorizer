namespace OtpAuthorizer.Shared;

public sealed class AppSettings
{
    public required OtpConfigurations OtpConfigurations { get; set; }
}


public sealed class OtpConfigurations
{
    public required List<OptChannelConfiguration> Channels { get; set; }
}

public sealed class OptChannelConfiguration
{
    public OtpChannel Name { get; set; }
    public int Length { get; set; }
    public int MaxRepeatNumber { get; set; }
    public bool StartWithZero { get; set; }
    public int ExpireOnMinutes { get; set; }
}



 