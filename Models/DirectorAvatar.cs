using TomorrowsVoice_Toplevel.Models.Users.Account;

namespace TomorrowsVoice_Toplevel.Models
{
	public class DirectorAvatar : Avatar
	{
		public int DirectorID { get; set; }
		public Director? Director { get; set; }
	}
}