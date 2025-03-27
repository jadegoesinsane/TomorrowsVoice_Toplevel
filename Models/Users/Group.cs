using System.ComponentModel.DataAnnotations;
using TomorrowsVoice_Toplevel.Models.Volunteering;
using TomorrowsVoice_Toplevel.Utilities;

namespace TomorrowsVoice_Toplevel.Models.Users
{
	public class Group
	{
		public int ID { get; set; }

		[Required(ErrorMessage = "Please enter a name for this group.")]
		[StringLength(55)]
		public string Name { get; set; } = "";

		[Display(Name = "Description")]
		[StringLength(255)]
		[DataType(DataType.MultilineText)]
		public string Description { get; set; } = "";

		[Display(Name = "Created On")]
		public DateTime CreatedOn { get; } = DateTime.Now;

		[Display(Name = "Group Type")]
		[StringLength(55)]
		[DisplayFormat(NullDisplayText = "None")]
		public string? GroupType { get; set; }

		public Status Status { get; set; } = Status.Active;
		public string BackgroundColour { get; set; } = ColourPalette.BrightColours["Blue"];

		public string TextColour
		{ get { return ColourPalette.GetTextColour(BackgroundColour); } }

		public ICollection<VolunteerGroup> VolunteerGroups { get; set; } = new List<VolunteerGroup>();
	}
}