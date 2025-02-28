using System.Net.Mail;

namespace TomorrowsVoice_Toplevel.ViewModels
{
	public class EmailMessage
	{
		public List<EmailAddress> ToAddresses { get; set; } = [];
		public string Subject { get; set; } = "";
		public string Content { get; set; } = "";
	}
}
