using System.ComponentModel.DataAnnotations;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models.Messaging;

namespace TomorrowsVoice_Toplevel.Models.Volunteering
{
	public class Shift
	{
		public int ID { get; set; }

        [Display(Name = "Date")]
        [Required(ErrorMessage = "Please select a date for this Shift")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ShiftDate { get; set; }

        [Required(ErrorMessage = "Shift requires a start time.")]
		[Display(Name = "Start Time")]
		[DataType(DataType.Time)]
		public DateTime StartAt { get; set; }

		[Required(ErrorMessage = "Shift requires an end time.")]
		[Display(Name = "End Time")]
		[DataType(DataType.Time)]
		public DateTime EndAt { get; set; }

		[Display(Name = "Shift Duration")]
		public TimeSpan ShiftDuration
		{
			get
			{
				return EndAt - StartAt;
			}
		}
        [Display(Name = "Status")]
        public Status Status { get; set; } = Status.Active;

		[Display(Name="Volunteers Needed")]
        public int VolunteersNeeded { get; set; }

		/*public int VolunteersSignedUp
		{
			get
			{
				int count = VolunteerShifts.Count();
				if (count > 0)
					return count;
				else
					return 0;
			}
		}*/

		public int EventID { get; set; }
		public Event? Event { get; set; }
		public virtual ICollection<UserShift> UserShifts { get; set; } = new HashSet<UserShift>();

		public void AddChat(TVContext context)
		{
			if (!context.Chats.Any(d => d.ID == ID))
			{
				Chat chat = new Chat
				{
					ID = ID,
					Shift = this,
					Title = $"Shift {ID}"
				};
				context.Chats.Add(chat);
				context.SaveChanges();
			}
		}

        public static explicit operator Shift(Task<Shift?> v)
        {
            throw new NotImplementedException();
        }
    }
}