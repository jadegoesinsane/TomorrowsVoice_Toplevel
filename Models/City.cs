using System.ComponentModel.DataAnnotations;

namespace TomorrowsVoice_Toplevel.Models
{
	public class City
	{
		public int ID { get; set; }

		[Display(Name = "Name")]
		[Required(ErrorMessage = "Please enter a city name.")]
		[StringLength(50, ErrorMessage = "City name cannot be more than 50 characters long.")]
		public string Name { get; set; }

		[Display(Name = "Province")]
		[Required(ErrorMessage = "Please select a province.")]
		public Province Province { get; set; }
	}
}