﻿using TomorrowsVoice_Toplevel.Models.Users.Account;

namespace TomorrowsVoice_Toplevel.Models.Users
{
	public class User : Person
	{
		public ICollection<Role> Roles { get; set; } = new HashSet<Role>();
		public string? Nickname { get; set; }
	}
}