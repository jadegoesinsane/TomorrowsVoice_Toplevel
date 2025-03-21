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

		[Display(Name = "Roles")]
		public List<string> UserRoles { get; set; } = new List<string>();

		[Display(Name = "Tags")]
		public ICollection<VolunteerGroup> Tags { get; set; } = new List<VolunteerGroup>();
	}
}