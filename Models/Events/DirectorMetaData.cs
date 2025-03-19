using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models.Users;
using TomorrowsVoice_Toplevel.Models.Volunteering;

namespace TomorrowsVoice_Toplevel.Models.Events
{
	public class DirectorMetaData : PersonMetaData
	{
		[Display(Name = "Chapter")]
		[Required(ErrorMessage = "Please select a Chapter")]
		public int ChapterID { get; set; }

		public Chapter? Chapter { get; set; }

		public virtual ICollection<Rehearsal> Rehearsals { get; set; } = new HashSet<Rehearsal>();

		[Display(Name = "Documents")]
		public ICollection<DirectorDocument> Documents { get; set; } = new HashSet<DirectorDocument>();

		public virtual ICollection<UserShift> UserShifts { get; set; } = new HashSet<UserShift>();
	}
}