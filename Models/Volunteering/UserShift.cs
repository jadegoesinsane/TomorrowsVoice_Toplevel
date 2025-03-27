using System.ComponentModel.DataAnnotations;
using TomorrowsVoice_Toplevel.Models.Users;

namespace TomorrowsVoice_Toplevel.Models.Volunteering
{
	public class UserShift
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
	}
}