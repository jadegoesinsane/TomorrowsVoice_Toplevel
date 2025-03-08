namespace TomorrowsVoice_Toplevel.Models.Volunteering
{
	public class ShiftJson
	{
		public string? Title { get; set; }
		public string? Start { get; set; }
		public string? End { get; set; }
		public string? BackgroundColor { get; set; }
		public string? BorderColor { get; set; }
		public string? TextColor { get; set; }
		public ExtendedProps? ExtendedProps { get; set; }
	}

	public class ExtendedProps
	{
		public ExtendedProps()
		{
			VolunteersNeeded = VolunteersNeeded ?? 0;
		}
		public int? VolunteersNeeded { get; set; }
		public string? Note { get; set; }
		public string? Location { get; set; }
	}
}