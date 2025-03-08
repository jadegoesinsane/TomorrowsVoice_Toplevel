using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System.ComponentModel.DataAnnotations;
using TomorrowsVoice_Toplevel.Data;

namespace TomorrowsVoice_Toplevel.Models.Volunteering
{
	public class Shift
	{
		public int ID { get; set; }

		public string TimeFormat
		{
			get
			{
				string start = StartAt.ToString(StartAt.Minute == 0 ? "%h" : "h:mm");
				string end = EndAt.ToString(EndAt.Minute == 0 ? "%h" : "h:mm");
				string startTT = StartAt.ToString("tt").ToLower();
				string endTT = EndAt.ToString("tt").ToLower();

				if (startTT == endTT && StartAt.Date == EndAt.Date)
				{
					return $"{start} - {end}{endTT}";
				}
				else
				{
					return $"{start}{startTT} - {end}{endTT}";
				}
			}
		}

		public String DateFormat => ShiftDate.ToString("dddd, MMMM d");
		public String TimeSummary => $"{DateFormat} ⋅ {TimeFormat}";

		[StringLength(55)]
		[DisplayFormat(NullDisplayText = "(No title)")]
		public string? Title { get; set; }

		public string? Location { get; set; }

		[Display(Name = "Notes")]
		[StringLength(255)]
		[DataType(DataType.MultilineText)]
		public string? Note { get; set; }

		public string BackgroundColor { get; set; } = "#467ECE";
		public string TextColor { get; set; } = "#FFFFFF";

		[Display(Name = "Date")]
		[Required(ErrorMessage = "Please select a date for this Shift")]
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
		public DateTime ShiftDate { get; set; }

		[Required(ErrorMessage = "Shift requires a start time.")]
		[Display(Name = "Start Time")]
		[DataType(DataType.Time)]
		public DateTime StartAt { get; set; }

		[Required(ErrorMessage = "Shift requires an end time.")]
		[Display(Name = "End Time")]
		[DataType(DataType.Time)]
		public DateTime EndAt { get; set; }

		[Display(Name = "Shift Duration")]
		public TimeSpan ShiftDuration
		{
			get
			{
				return EndAt - StartAt;
			}
		}

		[Display(Name = "Status")]
		public Status Status { get; set; } = Status.Active;

		[Display(Name = "Volunteers Needed")]
		[Required(ErrorMessage = " Requires Volunteers.")]
		[Range(0, int.MaxValue, ErrorMessage = "Volunteers needed must be a positive value.")]
		public int VolunteersNeeded { get; set; }

		public int VolunteersLeft
		{
			get
			{
				int count = VolunteersNeeded - UserShifts.Count();
				if (count <= 0)
					return 0;
				else
					return count;
			}
		}

		/*public int VolunteersSignedUp
		{
			get
			{
				int count = VolunteerShifts.Count();
				if (count > 0)
					return count;
				else
					return 0;
			}
		}*/

		public int EventID { get; set; }
		public Event? Event { get; set; }
		public virtual ICollection<UserShift> UserShifts { get; set; } = new HashSet<UserShift>();

		public static explicit operator Shift(Task<Shift?> v)
		{
			throw new NotImplementedException();
		}
	}
}