
using MimeKit.Text;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using TomorrowsVoice_Toplevel.ViewModels;

namespace TomorrowsVoice_Toplevel.Utilities
{
	public class EmailService
	{
		/// <summary>
		/// This implements the IEmailService from
		/// Microsoft.AspNetCore.Identity.UI.Services for the Identity System
		/// Note that you may want to use https://mailtrap.io for testing email functionality
		/// </summary>
		public class EmailSender : IEmailSender
		{
			private readonly IEmailConfiguration _emailConfiguration;
			private readonly ILogger<EmailSender> _logger;

			public EmailSender(IEmailConfiguration emailConfiguration, ILogger<EmailSender> logger)
			{
				_emailConfiguration = emailConfiguration;
				_logger = logger;
			}

			/// <summary>
			/// Asynchronously builds and sends a message to a single recipient.
			/// </summary>
			/// <param name="email"></param>
			/// <param name="subject"></param>
			/// <param name="htmlMessage"></param>
			/// <returns></returns>
			public async Task SendEmailAsync(string email, string subject, string htmlMessage)
			{
				var message = new MimeMessage();
				message.To.Add(new MailboxAddress(email, email));
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
}
