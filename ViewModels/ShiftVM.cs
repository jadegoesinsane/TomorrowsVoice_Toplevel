using Org.BouncyCastle.Bcpg.OpenPgp;

namespace TomorrowsVoice_Toplevel.ViewModels
{
	public class ShiftVM
	{
		public int ID;
		public string Title 
		{ 
			get
			{ 
				if (_Title == null)
					return Event;
				else
					return this.Title;
			} 
			set
			{ 
				_Title = value;
			} 
		}
		private string? _Title;
		public string Event;
		public string Date;
	}
}
