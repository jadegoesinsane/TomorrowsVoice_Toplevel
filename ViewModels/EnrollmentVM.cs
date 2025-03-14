﻿using System.ComponentModel.DataAnnotations;

namespace TomorrowsVoice_Toplevel.ViewModels
{
    public class EnrollmentVM
    {
        public int UserID { get; set; }
        public string Volunteer { get; set; } = "";

        [Display(Name = "Shows up")]
        public bool ShowOrNot { get; set; } = false;



        public TimeSpan StartAt { get; set; }



        public TimeSpan EndAt { get; set; }

        public int ShiftID { get; set; }
    }
}
