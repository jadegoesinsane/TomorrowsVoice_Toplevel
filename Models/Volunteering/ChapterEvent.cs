namespace TomorrowsVoice_Toplevel.Models.Volunteering
{
	public class ChapterEvent
	{
		public int ChapterID { get; set; }

		public Chapter? Chapter { get; set; }

		public int EventID { get; set; }
		public Event? Event { get; set; }
	}
}