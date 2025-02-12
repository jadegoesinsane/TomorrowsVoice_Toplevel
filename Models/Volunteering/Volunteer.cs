namespace TomorrowsVoice_Toplevel.Models.Volunteering
{
	public class Volunteer : Person
	{
		public int ID { get; set; }
		public virtual ICollection<VolunteerShift> VolunteerShifts { get; set; } = new HashSet<VolunteerShift>();
	}
}