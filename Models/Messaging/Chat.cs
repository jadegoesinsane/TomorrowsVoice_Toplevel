using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models.Volunteering;

namespace TomorrowsVoice_Toplevel.Models.Messaging
{
	public class Chat
	{
		public int ID { get; set; }
		public int ShiftID { get; set; }
		public Shift? Shift { get; set; }

		public string Title { get; set; } = "Unknown";

		public ICollection<ChatVolunteer> ChatVolunteers { get; set; } = new HashSet<ChatVolunteer>();
		public ICollection<Message> Messages { get; set; } = new HashSet<Message>();

		public void AddMessage(TVContext context, Volunteer volunteer, string content)
		{
			Message message = new Message
			{
				FromAccountID = volunteer.ID,
				Volunteer = volunteer,
				ChatID = this.ID,
				Content = content
			};
			context.Messages.Add(message);
			context.SaveChanges();
		}
	}
}