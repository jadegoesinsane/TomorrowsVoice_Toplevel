using System.ComponentModel.DataAnnotations;

namespace TomorrowsVoice_Toplevel.Models
{
	public class Singer : Person
	{
		public int ID { get; set; }

		[Display(Name = "Emergency Contact Info")]
		public string EmergencySummary
		{
			get
			{
				return $"{ContactName} - {PhoneFormatted}";
			}
		}

		[Display(Name = "Emergency Contact")]
		[Required(ErrorMessage = "You cannot leave the name blank.")]
		[MaxLength(100, ErrorMessage = "Name cannot be more than 100 characters long.")]
		public string ContactName { get; set; } = "";

		[Display(Name = "Notes")]
		[StringLength(255)]
		[DataType(DataType.MultilineText)]
		public string? Note { get; set; }

		[Display(Name = "Guardian Email")]
		[RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please follow the correct email format example@email.com")]
		[StringLength(255)]
		[DataType(DataType.EmailAddress)]
		public new string? Email { get; set; } = "";

		[Display(Name = "Emergency Contact Number")]
		[Required(ErrorMessage = "Emergency contact number is required.")]
		[RegularExpression("^\\d{10}$", ErrorMessage = "Please enter a valid 10-digit phone number (no spaces).")]
		[DataType(DataType.PhoneNumber)]
		[MaxLength(10)]
		public override string Phone { get; set; } = "";

		[Display(Name = "Chapter")]
		[Required(ErrorMessage = "You must select a Chapter")]
		public int ChapterID { get; set; }

		public Chapter? Chapter { get; set; }

		public ICollection<RehearsalAttendance> RehearsalAttendances { get; set; } = new HashSet<RehearsalAttendance>();
	}
}