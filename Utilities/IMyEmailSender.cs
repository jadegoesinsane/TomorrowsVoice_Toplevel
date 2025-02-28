using TomorrowsVoice_Toplevel.ViewModels;

namespace TomorrowsVoice_Toplevel.Utilities
{
	public interface IMyEmailSender
	{
		Task SendOneAsync(string name, string email, string subject, string htmlMessage);
		Task SendToManyAsync(EmailMessage emailMessage);
	}
}
