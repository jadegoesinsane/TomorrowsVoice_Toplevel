using System.ComponentModel.DataAnnotations;

namespace TomorrowsVoice_Toplevel.Models
{
	public abstract class Auditable : IAuditable
	{
		[ScaffoldColumn(false)]
		[StringLength(256)]
		public string? CreatedBy { get; set; }

		[ScaffoldColumn(false)]
		public DateTime? CreatedOn { get; set; }

		[ScaffoldColumn(false)]
		[StringLength(256)]
		public string? UpdatedBy { get; set; }

		[ScaffoldColumn(false)]
		public DateTime? UpdatedOn { get; set; }

		// Concurrency
		[ScaffoldColumn(false)]
		[Timestamp]
		public Byte[]? RowVersion { get; set; }
	}
}