using System.ComponentModel.DataAnnotations;

namespace TomorrowsVoice_Toplevel.ViewModels
{
    public class AttendanceSummaryVM
    {
        [Display(Name = "City")]
        public string? City { get; set; }


        [Display(Name = "Number Of Rehearsals")]
        public int Number_Of_Rehearsals { get; set; }

        [Display(Name = "Average Attendance")]

        public double Average_Attendance { get; set; }

        [Display(Name = "Highest Attendance")]

        public double Highest_Attendance { get; set; }

        [Display(Name = "Lowest Attendance")]

        public double Lowest_Attendance { get; set; }

        [Display(Name = "Total Attendance")]

        public double Total_Attendance { get; set; }

    }
}



