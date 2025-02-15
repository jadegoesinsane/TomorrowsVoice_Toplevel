using TomorrowsVoice_Toplevel.Models.Users;
using TomorrowsVoice_Toplevel.Models.Users.Account;

namespace TomorrowsVoice_Toplevel.Models.Volunteering
{
	public class Volunteer : User
	{
		public int ID { get; set; }
		public VolunteerAvatar? Avatar { get; set; }
		public virtual ICollection<VolunteerShift> VolunteerShifts { get; set; } = new HashSet<VolunteerShift>();
	}
}