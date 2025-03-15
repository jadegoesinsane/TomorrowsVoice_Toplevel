using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TomorrowsVoice_Toplevel.Models.Users;

namespace TomorrowsVoice_Toplevel.ViewModels
{
	[ModelMetadataType(typeof(PersonMetaData))]
	public class PersonVM
	{
		public int ID { get; set; }

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

		public string PhoneFormatted => "(" + Phone.Substring(0, 3) + ") " + Phone.Substring(3, 3) + "-" + Phone.Substring(6, 4);

		public string FirstName { get; set; } = "";
		public string? MiddleName { get; set; }
		public string LastName { get; set; } = "";
		public virtual string Email { get; set; } = "";
		public virtual string Phone { get; set; } = "";
	}
}