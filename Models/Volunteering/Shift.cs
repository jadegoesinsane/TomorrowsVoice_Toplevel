﻿using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Imaging;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Utilities;

namespace TomorrowsVoice_Toplevel.Models.Volunteering
{
	public class Shift : IValidatableObject
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
		[DisplayFormat(NullDisplayText = "(No Title)")]
		public string? Title { get; set; }

		public string? Location { get; set; }

		[Display(Name = "Notes")]
		[StringLength(255)]
		[DataType(DataType.MultilineText)]
		public string? Note { get; set; }

		public ColourScheme? Colour { get; set; }

		[Required(ErrorMessage = "Choose event colour palette.")]
		[Display(Name = "Colour Palette")]
		public int ColourID { get; set; } = 1;

		[Display(Name = "Date")]
		[Required(ErrorMessage = "Please select a date for this Shift")]
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
		public DateTime ShiftDate { get; set; }

		[Required(ErrorMessage = "Shift requires a start time.")]
		[Display(Name = "Start Time")]
		[DataType(DataType.Time)]
		public DateTime StartAt
		{
			get { return _startAt; }
			set { _startAt = new DateTime(ShiftDate.Year, ShiftDate.Month, ShiftDate.Day, value.Hour, value.Minute, value.Second); }
		}

		private DateTime _startAt;

		[Required(ErrorMessage = "Shift requires an end time.")]
		[Display(Name = "End Time")]
		[DataType(DataType.Time)]
		public DateTime EndAt
		{
			get { return _endAt; }
			set { _endAt = new DateTime(ShiftDate.Year, ShiftDate.Month, ShiftDate.Day, value.Hour, value.Minute, value.Second); }
		}

		private DateTime _endAt;

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
		[Required(ErrorMessage = "Must enter number of volunteers desired for this shift.")]
		[Range(0, int.MaxValue, ErrorMessage = "Volunteers desired must be a positive value.")]
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

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (EndAt < StartAt)
				yield return new ValidationResult("Shift cannot end before it starts.", new[] { "EndAt" });
		}
	}
}