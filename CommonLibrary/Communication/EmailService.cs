using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace Communication
{
    /*
      include below JSON on appsettings.json
    
    "Smtp": {
      "Host": "smtp.yourdomain.com",
      "Port": "587",
      "Username": "smtp-user",
      "Password": "smtp-password",
      "EnableSsl": "true"
    }

     */


    /*
     * Sample Code
     var message = new MailMessage("no-reply@yourdomain.com", user.EmailId)
{
    Subject = "Password Changed",
    Body = "Your password was changed successfully.",
    IsBodyHtml = false
};

var success = await _emailService.SendEmailAsync(message);

if (!success)
{
    // Optionally log or alert on failure
}
     */
    public class EmailService
    {
        private readonly SmtpClient _smtpClient;

        public EmailService(IConfiguration configuration)
        {
            // Fetch from config or hardcode as needed
            _smtpClient = new SmtpClient(configuration["Smtp:Host"])
            {
                Port = int.Parse(configuration["Smtp:Port"]),
                Credentials = new NetworkCredential(configuration["Smtp:Username"], configuration["Smtp:Password"]),
                EnableSsl = bool.Parse(configuration["Smtp:EnableSsl"])
            };
        }

        public async Task<bool> SendEmailAsync(MailMessage message)
        {
            try
            {
                await _smtpClient.SendMailAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                // Optionally log the error
                Console.WriteLine($"Email sending failed: {ex.Message}");
                return false;
            }
        }
    }
}