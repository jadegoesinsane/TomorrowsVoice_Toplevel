using TomorrowsVoice_Toplevel.Models;
using TomorrowsVoice_Toplevel.Models.Events;

namespace TomorrowsVoice_Toplevel.ViewModels
{
	public class HomeAdminVM
	{
		#region Monthly Volunteering Data

		public int VolunteerHours;
		public int VolunteerHours_Change;
		public int ShiftsFilled;
		public int ShiftsFilled_Change;
		public int NewVolunteers;

		#endregion Monthly Volunteering Data

		#region Monthly Choir Data

		public int Rehearsals;
		public int Attendance;
		public int NewSingers;

		#endregion Monthly Choir Data

		#region Recent Activity

		public List<TransactionVM> Transactions = new List<TransactionVM>();

		#endregion Recent Activity
	}
}