using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models.Users;

namespace TomorrowsVoice_Toplevel.Models.Volunteering
{
	[ModelMetadataType(typeof(VolunteerMetaData))]
	public class Volunteer : Person, IValidatableObject
	{
		[Display(Name = "Hours Volunteered")]
		public int HoursVolunteered
		{
			get { return (int)TotalWorkDuration.TotalHours; }
		}

		[Display(Name = "Total Time Worked")]
		public TimeSpan TotalWorkDuration => TimeSpan.FromMinutes(UserShifts.Sum(us => us.Duration.TotalMinutes));

		//UserShifts.Select(us => us.Duration).Aggregate(TimeSpan.Zero, (total, next) => total.Add(next));

		//{
		//	get { return TimeSpan.Parse(_TotalWorkDuration); }
		//	private set { _TotalWorkDuration = value.ToString(); }
		//}

		//private string _TotalWorkDuration = TimeSpan.Zero.ToString();

		[Display(Name = "Time Worked This Year")]
		public TimeSpan YearlyWorkDuration => UserShifts.Select(us => us.Duration).Aggregate(TimeSpan.Zero, (total, next) => total.Add(next));

		[Display(Name = "Shifts Attended")]
		public int ParticipationCount => UserShifts.Where(us => us.NoShow == false).Count();

		[Display(Name = "Shifts Missed")]
		public int absences => UserShifts.Where(us => us.NoShow == true).Count();

		[Display(Name = "Yearly Volunteering Goal")]
		public int? YearlyVolunteerGoal { get; set; } = 0;

		public ICollection<UserShift> UserShifts { get; set; } = new HashSet<UserShift>();

		public ICollection<VolunteerGroup> VolunteerGroups { get; set; } = new List<VolunteerGroup>();

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (YearlyVolunteerGoal < 0)
				yield return new ValidationResult("Cannot set goal to a negative number.", new[] { "YearlyVolunteerGoal" });
		}
	}
}