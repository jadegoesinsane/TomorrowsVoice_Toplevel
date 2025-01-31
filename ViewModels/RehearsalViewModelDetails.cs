using System.ComponentModel.DataAnnotations;
using TomorrowsVoice_Toplevel.Models;

namespace TomorrowsVoice_Toplevel.ViewModels
{
    public class RehearsalViewModelDetails
    {

		public string City { get; set; }

		[Display(Name = "Rehearsal Date")]
		public DateTime Rehearsal_Date { get; set; }


		[Display(Name = "Number Of Singers")]
		public int Number_Of_Singers { get; set; }


        [Display(Name = "Attendance Rate")]

        public string  Attendance_Rate { get; set; }
    }
}
