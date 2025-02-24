namespace TomorrowsVoice_Toplevel.Models.Volunteering
{
	public class UserShift
	{
		public int UserID { get; set; }
		public Volunteer? Volunteer { get; set; }
		public Director? Director { get; set; }
		public int ShiftID { get; set; }
		public Shift? Shift { get; set; }
        public bool ShowOrNot { get; set; } = false;



        public TimeSpan StartAt { get; set; }



        public TimeSpan EndAt { get; set; }
    }
}