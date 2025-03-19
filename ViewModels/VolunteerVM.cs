using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TomorrowsVoice_Toplevel.Models;
using TomorrowsVoice_Toplevel.Models.Volunteering;

namespace TomorrowsVoice_Toplevel.ViewModels
{
	[ModelMetadataType(typeof(VolunteerMetaData))]
	public class VolunteerVM
	{
		public int ID { get; set; }

		#region Person Data

		public string NameFormatted => FirstName + (string.IsNullOrEmpty(MiddleName) ? " "
			: (" " + (char?)MiddleName[0] + ". ").ToUpper()) + LastName;

		public string PhoneFormatted => "(" + Phone?.Substring(0, 3) + ") "
			+ Phone?.Substring(3, 3) + "-" + Phone?[6..];

		public string FirstName { get; set; } = "";
		public string? MiddleName { get; set; }
		public string LastName { get; set; } = "";
		public virtual string Phone { get; set; } = "";

		#endregion Person Data

		#region Volunteer Data

		public int? YearlyVolunteerGoal { get; set; } = 0;
		//public virtual ICollection<UserShift> UserShifts { get; set; } = new HashSet<UserShift>();
		//public ICollection<VolunteerTags> Tags { get; set; } = new List<VolunteerTags>();

		#endregion Volunteer Data
	}
}