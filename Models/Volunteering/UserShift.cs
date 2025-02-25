using TomorrowsVoice_Toplevel.Models.Users;

namespace TomorrowsVoice_Toplevel.Models.Volunteering
{
	public class UserShift
	{
		public int UserID { get; set; }
		public User? User { get; set; }
		
		public int ShiftID { get; set; }
		public Shift? Shift { get; set; }
        public bool ShowOrNot { get; set; } = false;

        public TimeSpan StartAt { get; set; }
        public TimeSpan EndAt { get; set; }
    }
}