using System.ComponentModel.DataAnnotations;
using TomorrowsVoice_Toplevel.Utilities;

namespace TomorrowsVoice_Toplevel.Models.Users
{
	public class Tag
	{
		public int ID { get; set; }
		public string Title { get; set; } = "";

		[Display(Name = "Background Colour")]
		public string BackgroundColour { get; set; } = ColourPalette.BrightColours["Blue"];

		[Display(Name = "Text Colour")]
		public string TextColour => ColourPalette.GetTextColour(BackgroundColour);
	}
}