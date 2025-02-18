using TomorrowsVoice_Toplevel.Models.Volunteering;

namespace TomorrowsVoice_Toplevel.ViewModels
{
	public class MessageVM
	{
		public string Content { get; set; }
		public DateTime CreatedOn { get; set; }
		public string Name { get; set; }
		public VolunteerAvatar Avatar { get; set; }
	}
}