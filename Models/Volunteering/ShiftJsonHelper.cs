namespace TomorrowsVoice_Toplevel.Models.Volunteering
{
	public class ShiftJson
	{
		public string Title { get; set; }
		public string Start { get; set; }
		public string End { get; set; }
		public string BackgroundColor { get; set; }
		public ExtendedProps ExtendedProps { get; set; }
	}

	public class ExtendedProps
	{
		public int VolunteersNeeded { get; set; }
	}
}