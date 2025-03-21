using System.ComponentModel.DataAnnotations;
using TomorrowsVoice_Toplevel.Models.Volunteering;
using TomorrowsVoice_Toplevel.Utilities;

namespace TomorrowsVoice_Toplevel.Models.Users
{
	public class Group
	{
		public int ID { get; set; }
		public string Name { get; set; } = "";
		public ICollection<VolunteerGroup> VolunteerGroups { get; set; } = new List<VolunteerGroup>();
	}
}