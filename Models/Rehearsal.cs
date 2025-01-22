using System.ComponentModel.DataAnnotations;

namespace TomorrowsVoice_Toplevel.Models
{
	public class Rehearsal
	{
		public int ID { get; set; }

		public string Summary
		{
			get
			{
				return $"{RehearsalDate:MMMM dd, yyyy} at {StartTime:hh:mm tt}";
			}
		}

		public string TimeSummary
		{
			get
			{
				return $"{StartTime:hh:mm} to {EndTime:hh:mm tt}";
			}
		}

		[Required(ErrorMessage = "Please select a date for this rehearsal")]
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
		public DateTime RehearsalDate { get; set; }

		[Required(ErrorMessage = "Please select a start time for this rehearsal")]
		[Display(Name = "Start Time")]
		[DataType(DataType.Time)]
		public DateTime StartTime { get; set; }

		[Required(ErrorMessage = "Please select a end time for this rehearsal")]
		[Display(Name = "End Time")]
		[DataType(DataType.Time)]
		public DateTime EndTime { get; set; }

		[Display(Name = "Notes")]
		[StringLength(255)]
		[DataType(DataType.MultilineText)]
		public string Note { get; set; } = "";

		[Display(Name = "Director")]
		public int DirectorID { get; set; }

		public Director? Director { get; set; }

		public ICollection<RehearsalAttendance> RehearsalAttendances { get; set; } = new HashSet<RehearsalAttendance>();
	}
}