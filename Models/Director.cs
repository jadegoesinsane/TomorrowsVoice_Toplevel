using Microsoft.AspNetCore.Http.Connections;
using System.ComponentModel.DataAnnotations;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models.Users;
using TomorrowsVoice_Toplevel.Models.Users.Account;
using TomorrowsVoice_Toplevel.Models.Volunteering;

namespace TomorrowsVoice_Toplevel.Models
{
	public class Director : User
	{
		public Director()
		{ }

		//public Director(TVContext context)
		//{
		//	ID = context.GetNextID();
		//}

		//#region Summary Properties

		//[Display(Name = "Phone Number")]
		//public string PhoneFormatted => "(" + Phone.Substring(0, 3) + ") "
		//	+ Phone.Substring(3, 3) + "-" + Phone[6..];

		//#endregion Summary Properties

		//public int ID { get; set; }
		public DirectorAvatar? Avatar { get; set; }

		[Display(Name = "Chapter")]
		[Required(ErrorMessage = "Please select a Chapter")]
		public int ChapterID { get; set; }

		public Chapter? Chapter { get; set; }

		public virtual ICollection<Rehearsal> Rehearsals { get; set; } = new HashSet<Rehearsal>();

		[Display(Name = "Vulnerable Sector Checks")]
		public ICollection<DirectorDocument> VulnerableSectorChecks { get; set; } = new HashSet<DirectorDocument>();

		public virtual ICollection<UserShift> UserShifts { get; set; } = new HashSet<UserShift>();
	}
}