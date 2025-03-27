using System.ComponentModel.DataAnnotations;

namespace TomorrowsVoice_Toplevel.ViewModels
{
	public class EnrollmentVM
	{
		public int UserID { get; set; }
		public string Volunteer { get; set; } = "";

		[Display(Name = "Shows up")]
		public bool ShowOrNot { get; set; } = false;

		[DataType(DataType.Time)]
		public DateTime StartAt { get; set; }

		[DataType(DataType.Time)]
		public DateTime EndAt { get; set; }

		public int ShiftID { get; set; }
	}
}