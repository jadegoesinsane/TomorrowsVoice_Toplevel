using TomorrowsVoice_Toplevel.Models.Events;

namespace TomorrowsVoice_Toplevel.ViewModels
{
	public class SingersVM
	{

		public string FirstName { get; set; } = "";
		public string? MiddleName { get; set; }
		public string LastName { get; set; } = "";

		public string? Email { get; set; }

		public string ContactName { get; set; } = "";


		public string Phone { get; set; } = "";

		public Chapter? Chapter { get; set; }

		public string? Note { get; set; }
	}
}
