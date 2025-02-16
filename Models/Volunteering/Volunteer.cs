using TomorrowsVoice_Toplevel.Models.Users;
using TomorrowsVoice_Toplevel.Models.Users.Account;

namespace TomorrowsVoice_Toplevel.Models.Volunteering
{
	public class Volunteer : User
	{
		//public int ID { get; set; }
		public VolunteerAvatar? Avatar { get; set; }

		public int HoursVolunteered = 0;

		public string CurrentYearVolunteerTotal
		{
			get
			{
				TimeSpan total = new TimeSpan();
				foreach (TimeSpan item in VolunteerShifts.Select(vs => vs.Shift.ShiftDuration))
					total.Add(item);
				return string.Format("{0:D2}:{1:D2}", total.Hours, total.Minutes);
			}
		}

		public int? YearlyVolunteerGoal;
		public virtual ICollection<VolunteerShift> VolunteerShifts { get; set; } = new HashSet<VolunteerShift>();
	}
}