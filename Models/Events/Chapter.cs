using System.ComponentModel.DataAnnotations;
using TomorrowsVoice_Toplevel.Models.Volunteering;

namespace TomorrowsVoice_Toplevel.Models.Events
{
	public class Chapter : Auditable
	{
		public int ID { get; set; }

		/*public string Name
		{
			get
			{
				return City.Name;
			}
		}*/

		[Display(Name = "Address Summary")]
		public string AddressSummary
		{
			get
			{
				return Address + ", " + /*Name + ", " + Province + ", " +*/ (PostalCode.Length == 6 ? PostalCode.Substring(0, 3) + " " + PostalCode.Substring(3)
					: PostalCode.Substring(0, 3) + " " + PostalCode.Substring(4));
			}
		}

		[Display(Name = "Postal Code")]
		public string PostalCodeFormat
		{
			get
			{
				return PostalCode.Length == 6 ? PostalCode.Substring(0, 3) + " " + PostalCode.Substring(3)
					: PostalCode.Substring(0, 3) + " " + PostalCode.Substring(4);
			}
		}

		[Display(Name = "City")]
		[Required(ErrorMessage = "Please select a city.")]
		public int CityID { get; set; }

		public City? City { get; set; }

		[Display(Name = "Address")]
		[Required(ErrorMessage = "Please enter an address.")]
		[StringLength(50, ErrorMessage = "Address cannot be more than 50 characters long.")]
		public string Address { get; set; } = "";

		[Display(Name = "Postal Code")]
		[Required(ErrorMessage = "Please enter a postal code.")]
		[RegularExpression(@"^[A-Za-z]\d[A-Za-z][ -]?\d[A-Za-z]\d$", ErrorMessage = "Postal Code must follow the format: A1A 2B2.")]
		public string PostalCode { get; set; } = "";

		// status for archiving purposes
		public Status Status { get; set; } = Status.Active;

		public virtual ICollection<Director> Directors { get; set; } = new HashSet<Director>();
		public virtual ICollection<Rehearsal> Rehearsals { get; set; } = new HashSet<Rehearsal>();
		public virtual ICollection<Singer> Singers { get; set; } = new HashSet<Singer>();
		//public virtual ICollection<Volunteer> Volunteers { get; set; } = new HashSet<Volunteer>();
	}
}