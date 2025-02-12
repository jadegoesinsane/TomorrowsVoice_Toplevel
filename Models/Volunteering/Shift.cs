using System.ComponentModel.DataAnnotations;

namespace TomorrowsVoice_Toplevel.Models.Volunteering
{
	public class Shift
	{
		public int ID { get; set; }

		[Required(ErrorMessage = "Shift requires a start time.")]
		[Display(Name = "Start Time")]
		[DataType(DataType.DateTime)]
		public DateTime StartAt { get; set; }

		[Required(ErrorMessage = "Shift requires an end time.")]
		[Display(Name = "End Time")]
		[DataType(DataType.DateTime)]
		public DateTime EndAt { get; set; }

		public int VolunteersNeeded { get; set; }
		public int VolunteersSignedUp { get; set; } = 0;

		public int EventID { get; set; }
		public Event? Event { get; set; }
		public virtual ICollection<VolunteerShift> VolunteerShifts { get; set; } = new HashSet<VolunteerShift>();
	}
}