using System.ComponentModel.DataAnnotations;
using TomorrowsVoice_Toplevel.Models.Users;

namespace TomorrowsVoice_Toplevel.Models.Volunteering
{
	public class UserShift : IValidatableObject
	{
		public int UserID { get; set; }
		public Volunteer? User { get; set; }

		public int ShiftID { get; set; }
		public Shift? Shift { get; set; }
		public bool NoShow { get; set; } = false;

		[DataType(DataType.Time)]
		public DateTime StartAt { get; set; }

		[DataType(DataType.Time)]
		public DateTime EndAt { get; set; }

		public TimeSpan Duration => EndAt - StartAt;
		public bool WorkingHourRecorded { get; set; } = false;

		public bool HourRecordedVolunteer { get; set; } = false;
		public bool SentNotice { get; set; } = false;

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (Shift?.UserShifts.Count() > Shift?.VolunteersNeeded)
				yield return new ValidationResult("Cannot have more volunteers than the maximum.", new[] { "UserID" });
			if (EndAt < StartAt)
				yield return new ValidationResult("Shift cannot end before it starts.", new[] { "EndAt" });
		}
	}
}