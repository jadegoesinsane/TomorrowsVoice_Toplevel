using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace TomorrowsVoice_Toplevel.Models
{
	public class DirectorDocument : UploadedFile
	{
		[Display(Name = "Director")]
		public int DirectorID { get; set; }

		public Director? Director { get; set; }
	}
}