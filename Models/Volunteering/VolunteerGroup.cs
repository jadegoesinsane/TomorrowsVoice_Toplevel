using TomorrowsVoice_Toplevel.Models.Users;

namespace TomorrowsVoice_Toplevel.Models.Volunteering
{
	public class VolunteerGroup
	{
		public int ID { get; set; }
		public int VolunteerID { get; set; }

		public Volunteer? Volunteer { get; set; }

		public int GroupID { get; set; }
		public Group? Group { get; set; }
	}
}