using System.ComponentModel.DataAnnotations;

namespace TomorrowsVoice_Toplevel.Models.Users.Account
{
	public class Avatar
	{
		public int ID { get; set; }

		[ScaffoldColumn(false)]
		public byte[]? Content { get; set; }

		[StringLength(255)]
		public string? MimeType { get; set; }
	}
}