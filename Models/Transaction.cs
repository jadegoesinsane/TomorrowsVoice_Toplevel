using System.ComponentModel.DataAnnotations;

namespace TomorrowsVoice_Toplevel.Models
{
	public class Transaction
	{
		public int ID { get; set; }
		public long ItemID { get; set; }

		[StringLength(256)]
		public string? ChangedBy { get; set; }

		[StringLength(256)]
		public string? ChangeType { get; set; }

		public DateTime ChangeTimestamp { get; set; }

		[StringLength(256)]
		public string? OldValue { get; set; }

		[StringLength(256)]
		public string? NewValue { get; set; }

		[StringLength(256)]
		public string? Property { get; set; }

		[StringLength(256)]
		public string? ItemType { get; set; }
	}
}