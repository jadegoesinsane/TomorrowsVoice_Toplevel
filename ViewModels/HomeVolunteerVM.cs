using TomorrowsVoice_Toplevel.Models.Volunteering;

namespace TomorrowsVoice_Toplevel.ViewModels
{
	public class HomeVolunteerVM
	{
		public string Name;

		public int HourlyGoal;
		public TimeSpan TimeWorked;

		public List<Shift> Shifts;
	}
}