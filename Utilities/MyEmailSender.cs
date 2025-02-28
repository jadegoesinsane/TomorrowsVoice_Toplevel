using MimeKit.Text;
using MimeKit;
using TomorrowsVoice_Toplevel.ViewModels;
using MailKit.Net.Smtp;

namespace TomorrowsVoice_Toplevel.Utilities
{
	public class MyEmailSender : IMyEmailSender
	{
		private readonly IEmailConfiguration _emailConfiguration;
		private readonly ILogger<MyEmailSender> _logger;


		public MyEmailSender(IEmailConfiguration emailConfiguration, ILogger<MyEmailSender> logger)
		{
			_emailConfiguration = emailConfiguration;
			_logger = logger;
		}

		/// <summary>
		/// Asynchronously builds and sends a message to a single recipient with optional name parameter
		/// </summary>        
		/// <param name="name">Optional - Uses the email if not supplied</param>
		/// <param name="email"></param>
		/// <param name="subject"></param>
		/// <param name="htmlMessage"></param>
		/// <returns></returns>
		public async Task SendOneAsync(string? name, string email, string subject, string htmlMessage)
		{
			if (String.IsNullOrEmpty(name))
			{
				name = email;
			}
			var message = new MimeMessage();
			message.To.Add(new MailboxAddress(name, email));
			message.From.Add(new MailboxAddress(_emailConfiguration.SmtpFromName, _emailConfiguration.SmtpUsername));

			message.Subject = subject;
			//We will say we are sending HTML. But there are options for plaintext etc. 
			message.Body = new TextPart(TextFormat.Html)
			{
				Text = htmlMessage
			};

			await SendEmailMessageAsync(message);
		}

		/// <summary>
		/// Asynchronously sends a message to a List of email addresses
		/// </summary>
		/// <param name="emailMessage"></param>
		/// <returns></returns>
		public async Task SendToManyAsync(EmailMessage emailMessage)
		{
			var message = new MimeMessage();
			message.To.AddRange(emailMessage.ToAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
			message.From.Add(new MailboxAddress(_emailConfiguration.SmtpFromName, _emailConfiguration.SmtpUsername));

			message.Subject = emailMessage.Subject;
			//We will say we are sending HTML. But there are options for plaintext etc. 
			message.Body = new TextPart(TextFormat.Html)
			{
				Text = emailMessage.Content
			};

			await SendEmailMessageAsync(message);
		}

		/// <summary>
		/// Sends the MimeMessage
		/// </summary>
		/// <param name="theMessage">The MimeMessage</param>
		/// <returns></returns>
		public async Task SendEmailMessageAsync(MimeMessage theMessage)
		{
			try
			{
				//Be careful that the SmtpClient class is the one from Mailkit not the framework!
				using var emailClient = new SmtpClient();
				//The last parameter here is to use SSL 
				emailClient.Connect(_emailConfiguration.SmtpServer, _emailConfiguration.SmtpPort, false);

				//Remove any OAuth functionality as we won't be using it. 
				emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

				emailClient.Authenticate(_emailConfiguration.SmtpUsername, _emailConfiguration.SmtpPassword);

				await emailClient.SendAsync(theMessage);

				emailClient.Disconnect(true);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.GetBaseException().Message);
			}
		}
	}
}
