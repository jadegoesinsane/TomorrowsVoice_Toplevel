using System.ComponentModel.DataAnnotations;

namespace TomorrowsVoice_Toplevel.Models.Volunteering
{
	public class Event
	{
		public int ID { get; set; }
		public string Name { get; set; }

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

		[Display(Name = "Status")]
		public Status Status { get; set; } = Status.Active;

		public virtual ICollection<ChapterEvent> ChapterEvents { get; set; } = new HashSet<ChapterEvent>();
		public ICollection<Shift> Shifts { get; set; } = new HashSet<Shift>();
	}
}