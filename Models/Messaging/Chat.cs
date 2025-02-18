using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models.Users;
using TomorrowsVoice_Toplevel.Models.Volunteering;

namespace TomorrowsVoice_Toplevel.Models.Messaging
{
	public class Chat
	{
		[Key]
		[ForeignKey("Shift")]
		public int ID { get; set; }

		public Shift Shift { get; set; }

		public string Title { get; set; } = "Unknown";

		public ICollection<ChatUser> ChatUsers { get; set; } = new HashSet<ChatUser>();
		public ICollection<Message> Messages { get; set; } = new HashSet<Message>();

		public void AddMessage(TVContext context, IUser user, string content)
		{
			Message message = new Message
			{
				FromAccountID = user.ID,
				ChatID = this.ID,
				Content = content,
				User = user
			};
			//if (user is Volunteer)
			//	message.Volunteer = user as Volunteer;
			//else if (user is Director)
			//	message.D = user as Volunteer;
			context.Messages.Add(message);
			context.SaveChanges();
		}
	}
}