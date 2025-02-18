using System.ComponentModel.DataAnnotations;
using TomorrowsVoice_Toplevel.Models.Users.Account;

namespace TomorrowsVoice_Toplevel.Models.Users
{
	public interface IUser
	{
		public int ID { get; set; }
		public string? Nickname { get; set; }
		public ICollection<Role> Roles { get; set; }

		public string NameFormatted
		{
			get
			{
				return FirstName
					+ (string.IsNullOrEmpty(MiddleName) ? " " :
						(" " + (char?)MiddleName[0] + ". ").ToUpper())
					+ LastName;
			}
		}

		public string FirstName { get; set; }
		public string? MiddleName { get; set; }
		public string LastName { get; set; }
	}
}