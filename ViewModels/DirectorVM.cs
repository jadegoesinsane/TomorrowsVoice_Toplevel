using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TomorrowsVoice_Toplevel.Models.Events;
using TomorrowsVoice_Toplevel.Models.Volunteering;

namespace TomorrowsVoice_Toplevel.ViewModels
{
	[ModelMetadataType(typeof(DirectorMetaData))]
	public class DirectorVM
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

		#region Director Data

		public virtual ICollection<Rehearsal> Rehearsals { get; set; } = new HashSet<Rehearsal>();
		public ICollection<DirectorDocument> Documents { get; set; } = new HashSet<DirectorDocument>();

		#endregion Director Data
	}
}