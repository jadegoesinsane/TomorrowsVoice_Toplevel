namespace TomorrowsVoice_Toplevel.Models.Users
{
	public class User : Person, IUser
	{
		public int ID { get; set; }
		public string? Nickname { get; set; }
		public ICollection<Role> Roles { get; set; } = new List<Role>();
	}
}