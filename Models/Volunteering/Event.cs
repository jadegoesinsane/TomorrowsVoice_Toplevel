using System.ComponentModel.DataAnnotations;

namespace TomorrowsVoice_Toplevel.Models.Volunteering
{
	public class Event
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public string DateSummary
		{
			get
			{
				if (StartDate.Month == EndDate.Month & StartDate.Year == EndDate.Year)
					return $"{StartDate.ToString("MMMM d")} - {EndDate.ToString("d, yyyy")}";
				else if (StartDate.Month != EndDate.Month & StartDate.Year == EndDate.Year)
					return $"{StartDate.ToString("MMMM d")} - {EndDate.ToString("MMMM d, yyyy")}";
				else
					return $"{StartDate.ToString("MMMM d, yyyy")} - {EndDate.ToString("MMMM d, yyyy")}";
			}
		}

		[Display(Name = "Start")]
		[Required(ErrorMessage = "Event requires a start date.")]
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
		public DateTime StartDate { get; set; }

		[Display(Name = "End")]
		[Required(ErrorMessage = "Event requires a end date.")]
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
		public DateTime EndDate { get; set; }

		public string Descripion { get; set; }

		public string Location { get; set; }

		[Display(Name = "Background Colour")]
		public string BackgroundColour { get; set; } = "#467ECE";

		[Display(Name = "Text Colour")]
		public string TextColour
		{
			get
			{
				var brightColors = new List<string> { "#467ECE", "#9944bc", "#d3162b", "#804205", "#aa394f" };
				var pastelColors = new List<string> { "#F6CBDF", "#D7E3C0", "#f5e0ac", "#BFD6E9", "#d8cbe7" };

				if (brightColors.Contains(this.BackgroundColour))
				{
					return "#FFFFFF";
				}
				else
				{
					return "#000000";
				}
			}
		}

		[Display(Name = "Status")]
		public Status Status { get; set; } = Status.Active;

		public virtual ICollection<CityEvent> CityEvents { get; set; } = new HashSet<CityEvent>();
		public ICollection<Shift> Shifts { get; set; } = new HashSet<Shift>();
	}
}