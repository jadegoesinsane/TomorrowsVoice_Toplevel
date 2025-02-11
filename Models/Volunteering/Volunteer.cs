namespace TomorrowsVoice_Toplevel.Models.Volunteering
{
	public class Volunteer : Person
	{
		public virtual ICollection<VolunteerShift> VolunteerShifts { get; set; } = new HashSet<VolunteerShift>();
	}
}