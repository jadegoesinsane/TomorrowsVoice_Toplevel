using TomorrowsVoice_Toplevel.Models.Volunteering;

namespace TomorrowsVoice_Toplevel.Models.Messaging
{
	public class Message
	{
		public int ID { get; set; }
		public string? Content { get; set; }
		public int DiscussionID { get; set; }
		public int FromAccountID { get; set; }
		public Volunteer? Volunteer { get; set; }
		public DateTime CreatedOn { get; set; }
	}
}