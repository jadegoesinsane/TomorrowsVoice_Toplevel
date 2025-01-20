﻿using System.ComponentModel.DataAnnotations;

namespace TomorrowsVoice_Toplevel.Models
{
    public class Chapter
    {
        public int ID { get; set; }

        [Display(Name = "Name")]
        [Required(ErrorMessage = "You cannot leave the name blank.")]
        [StringLength(50, ErrorMessage = "Name cannot be more than 50 characters long.")]
        public string Name { get; set; } = "";

        public string Address { get; set; } = "";

        public string City { get; set; } = "";
        public string Province { get; set; } = "";

        [Display(Name = "Postal Code")]
        public string Postal { get; set; } = "";

        public virtual ICollection<Director> Directors { get; set; } = new HashSet<Director>();
        
        public virtual ICollection<Singer> Singers { get; set; } = new HashSet<Singer>();
        //public virtual ICollection<Event> Events { get; set; } = new HashSet<Event>();
        //public virtual ICollection<Volunteer> Volunteers { get; set; } = new HashSet<Volunteer>();
    }
}
