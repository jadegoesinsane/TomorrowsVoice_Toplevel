using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TomorrowsVoice_Toplevel.Models;
using TomorrowsVoice_Toplevel.Models.Volunteering;

namespace TomorrowsVoice_Toplevel.ViewModels
{
	[ModelMetadataType(typeof(VolunteerMetaData))]
	public class VolunteerAdminVM : VolunteerVM
	{
		public string Email { get; set; } = "";
		public Status Status { get; set; } = Status.Active;

		[Display(Name = "Total Time Worked")]
		public double TotalWorkDuration { get; set; }

		[Display(Name = "Time Worked This Year")]
		public double YearlyWorkDuration { get; set; }

		[Display(Name = "Shifts Attended")]
		public int ParticipationCount;

		[Display(Name = "Shifts Missed")]
		public int absences;

		[Display(Name = "Roles")]
		public List<string> UserRoles { get; set; } = new List<string>();

		[Display(Name = "Tags")]
		public ICollection<VolunteerGroup> Tags { get; set; } = new List<VolunteerGroup>();
	}
}