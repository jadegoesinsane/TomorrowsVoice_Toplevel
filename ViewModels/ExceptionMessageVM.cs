namespace TomorrowsVoice_Toplevel.ViewModels
{
	public class ExceptionMessageVM
	{
		public string ErrProperty { get; set; } = String.Empty;

		public string ErrMessage { get; set; } = "Unable to save changes. " +
			"Try again, and if the problem persists see your system administrator.";
	}
}