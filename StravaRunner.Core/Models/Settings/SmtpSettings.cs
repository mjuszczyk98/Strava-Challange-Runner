namespace StravaRunner.Core.Models.Settings;

public class SmtpSettings
{
    public required string DefaultFromEmail { get; set; }
    public required string DefaultReplyToEmail { get; set; }
    public required string SmtpServer { get; set; }
    public required int Port { get; set; }
    public required bool EnableSsl { get; set; }
    public required SmtpCredentials Credentials { get; set; }
}

public class SmtpCredentials
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}