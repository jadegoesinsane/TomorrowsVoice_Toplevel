using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.ComponentModel.DataAnnotations;

namespace TomorrowsVoice_Toplevel.ViewModels
{
	public class TransactionVM
	{
		public int ID { get; set; }
		public long ItemID { get; set; }

		[StringLength(256)]
		public string? ChangedBy { get; set; }

		[StringLength(256)]
		public string? ChangedByType { get; set; }

		public int? ChangedByID { get; set; }

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

		public string? Description { get; set; }

		public string TimeSince()
		{
			TimeSpan since = DateTime.UtcNow - this.ChangeTimestamp;

			int hours = (int)since.TotalHours;
			int days = (int)since.TotalDays;

			if (since.TotalSeconds < 60)
				return $"{(int)since.TotalSeconds} sec";
			else if (since.TotalMinutes < 60)
				return $"{(int)since.TotalMinutes} min";
			else if (since.TotalHours < 24)
				return $"{hours} hr{(hours > 1 ? "s" : "")}";
			else if (since.TotalDays < 7)
				return $"{days} day{(days > 1 ? "s" : "")}";
			else
			{
				int weeks = (int)since.TotalDays / 7;
				days = (int)since.TotalDays % 7;
				if (weeks > 0 && days > 0)
					return $"{weeks} week{(weeks > 1 ? "s" : "")} and {days} day{(days > 1 ? "s" : "")}";
				else if (weeks > 0)
					return $"{weeks} week{(weeks > 1 ? "s" : "")}";
				else
					return $"{days} day{(days > 1 ? "s" : "")}";
			}
		}
	}
}