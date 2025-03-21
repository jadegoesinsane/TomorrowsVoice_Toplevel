using System.ComponentModel.DataAnnotations;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models.Users;

namespace TomorrowsVoice_Toplevel.Models.Volunteering
{
	public class VolunteerMetaData : PersonMetaData
	{
		[Display(Name = "Hours Volunteered")]
		public int HoursVolunteered
		{
			get { return (int)TotalWorkDuration.TotalHours; }
		}

		[Display(Name = "Total Work Duration")]
		public TimeSpan TotalWorkDuration { get; set; } = TimeSpan.Zero;

		[Display(Name = "Shifts Attended")]
		public int ParticipationCount { get; set; } = 0;

		[Display(Name = "Shifts Missed")]
		public int absences { get; set; } = 0;

		[Display(Name = "Yearly Volunteering Goal")]
		public int? YearlyVolunteerGoal { get; set; } = 0;

		public virtual ICollection<UserShift> UserShifts { get; set; } = new HashSet<UserShift>();
		public ICollection<VolunteerGroup> Tags { get; set; } = new List<VolunteerGroup>();
	}
}