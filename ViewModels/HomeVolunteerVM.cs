using TomorrowsVoice_Toplevel.Models.Volunteering;

namespace TomorrowsVoice_Toplevel.ViewModels
{
	public class HomeVolunteerVM
	{
		public string Name;

		// Times
		public int HourlyGoal;

		public TimeSpan TimeWorked;
		public int Progress;

		// Shifts
		public List<Shift> Shifts = new List<Shift>();
	}
}