namespace TomorrowsVoice_Toplevel.Models.Volunteering
{
	public class VolunteerShift
	{
		public int VolunteerID { get; set; }
		public Volunteer? Volunteer { get; set; }
		public int ShiftID { get; set; }
		public Shift? Shift { get; set; }

		
	}
}