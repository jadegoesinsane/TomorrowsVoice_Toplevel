using TomorrowsVoice_Toplevel.Models.Users.Account;

namespace TomorrowsVoice_Toplevel.Models.Volunteering
{
	public class VolunteerAvatar : Avatar
	{
		public int VolunteerID { get; set; }
		public Volunteer? Volunteer { get; set; }
	}
}