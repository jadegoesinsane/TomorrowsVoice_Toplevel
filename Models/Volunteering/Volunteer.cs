using System.ComponentModel.DataAnnotations;
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

		[Display(Name ="Hours Volunteered")]
		public int HoursVolunteered { get; set; }=0;

        [Display(Name = "Total Work Duration")]
        public TimeSpan totalWorkDuration { get; set; }=TimeSpan.Zero;

        [Display(Name = "Shifts Attended")]
        public int ParticipationCount { get; set; }=0;

        /*public string CurrentYearVolunteerTotal
		{
			get
			{
				TimeSpan total = new TimeSpan();
				foreach (TimeSpan item in UserShifts.Select(vs => vs.Shift.ShiftDuration))
					total.Add(item);
				return string.Format("{0:D2}:{1:D2}", total.Hours, total.Minutes);
			}
		}*/

        [Display(Name = "Shifts Missed")]
        public int absences { get; set; }=0;

        [Display(Name = "Yearly Volunteering Goal")]
        public int? YearlyVolunteerGoal { get; set; }=0;
		public virtual ICollection<UserShift> UserShifts { get; set; } = new HashSet<UserShift>();
	}
}