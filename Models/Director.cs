﻿using System.ComponentModel.DataAnnotations;

namespace TomorrowsVoice_Toplevel.Models
{
    public class Director : Person
    {
        #region Summary Properties

        [Display(Name = "Phone Number")]
        public string PhoneFormatted => "(" + Phone.Substring(0, 3) + ") "
            + Phone.Substring(3, 3) + "-" + Phone[6..];

        #endregion

        public int ID { get; set; }

        [Display(Name = "Chapter")]
        [Required(ErrorMessage = "You must select a Chapter")]
        public int ChapterID { get; set; }

        public Chapter? Chapter { get; set; }

        public virtual ICollection<Rehearsal> Rehearsals { get; set; } = new HashSet<Rehearsal>();

    }
}
