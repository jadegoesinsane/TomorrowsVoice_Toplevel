using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models.Volunteering;

namespace TomorrowsVoice_Toplevel.Models.Messaging
{
	public class Discussion
	{
		public int ID { get; set; }
		public int ShiftID { get; set; }
		public Shift? Shift { get; set; }

		public string Title { get; set; } = "Unknown";

		public ICollection<DiscussionVolunteer> DiscussionVolunteers { get; set; } = new HashSet<DiscussionVolunteer>();
		public ICollection<Message> Messages { get; set; } = new HashSet<Message>();

		public void AddMessage(TVContext context, Volunteer volunteer, string content)
		{
			Message message = new Message
			{
				FromAccountID = volunteer.ID,
				Volunteer = volunteer,
				DiscussionID = this.ID,
				Content = content,
				CreatedOn = DateTime.Now
			};
			context.Messages.Add(message);
			context.SaveChanges();
		}
	}
}