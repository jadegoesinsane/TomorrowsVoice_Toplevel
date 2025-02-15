namespace TomorrowsVoice_Toplevel.Models.Volunteering
{
	public class CityEvent
	{
		public int CityID { get; set; }

		public City? City { get; set; }

		public int EventID { get; set; }
		public Event? Event { get; set; }
	}
}