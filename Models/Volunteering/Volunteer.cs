using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models.Users;
using TomorrowsVoice_Toplevel.Models.Users.Account;

namespace TomorrowsVoice_Toplevel.Models.Volunteering
{
	public class Volunteer : User
	{
		public Volunteer()
		{ }

		//public Volunteer(TVContext context)
		//{
		//	ID = context.GetNextID();
		//}

		//public int ID { get; set; }
		public VolunteerAvatar? Avatar { get; set; }

		public int HoursVolunteered = 0;

		public string CurrentYearVolunteerTotal
		{
			get
			{
				TimeSpan total = new TimeSpan();
				foreach (TimeSpan item in UserShifts.Select(vs => vs.Shift.ShiftDuration))
					total.Add(item);
				return string.Format("{0:D2}:{1:D2}", total.Hours, total.Minutes);
			}
		}

		public int? YearlyVolunteerGoal;
		public virtual ICollection<UserShift> UserShifts { get; set; } = new HashSet<UserShift>();
	}
}