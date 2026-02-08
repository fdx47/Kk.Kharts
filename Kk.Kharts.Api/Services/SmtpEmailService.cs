using Kk.Kharts.Api.Services.IService;
using System.Net;
using System.Net.Mail;

public class SmtpEmailService : IEmailService
{
    private readonly SmtpClient _smtpClient;
    private readonly string _from;

    public SmtpEmailService(IConfiguration configuration)
    {
        // Lê do appsettings.json
        var smtpHost = configuration["Smtp:Host"] ?? throw new InvalidOperationException("Smtp:Host not configured");
        var smtpPort = int.Parse(configuration["Smtp:Port"] ?? throw new InvalidOperationException("Smtp:Port not configured"));
        var smtpUser = configuration["Smtp:Username"] ?? throw new InvalidOperationException("Smtp:Username not configured");
        var smtpPass = configuration["Smtp:Password"] ?? throw new InvalidOperationException("Smtp:Password not configured");
        _from = configuration["Smtp:From"] ?? throw new InvalidOperationException("Smtp:From not configured");

        _smtpClient = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPass),
            EnableSsl = true
        };
    }

    public async Task SendAsync(string to, string subject, string body)
    {
           var logs = new List<string>();
            var mail = new MailMessage(_from, to, subject, body);
            mail.IsBodyHtml = true;

            await _smtpClient.SendMailAsync(mail);
       
    }
}
