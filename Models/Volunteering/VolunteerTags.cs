using TomorrowsVoice_Toplevel.Models.Users;

namespace TomorrowsVoice_Toplevel.Models.Volunteering
{
	public class VolunteerTags
	{
		public int ID { get; set; }
		public int VolunteerID { get; set; }

		public Volunteer? Volunteer { get; set; }

		public int TagID { get; set; }
		public Tag? Tag { get; set; }
	}
}