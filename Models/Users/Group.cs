using System.ComponentModel.DataAnnotations;
using TomorrowsVoice_Toplevel.Models.Volunteering;
using TomorrowsVoice_Toplevel.Utilities;

namespace TomorrowsVoice_Toplevel.Models.Users
{
	public class Group
	{
		public int ID { get; set; }
		public string Name { get; set; } = "";

		[Display(Name = "Background Colour")]
		public string BackgroundColour { get; set; } = ColourPalette.BrightColours["Blue"];

		[Display(Name = "Text Colour")]
		public string TextColour => ColourPalette.GetTextColour(BackgroundColour);

		public ICollection<VolunteerGroup> VolunteerGroups { get; set; } = new List<VolunteerGroup>();
	}
}