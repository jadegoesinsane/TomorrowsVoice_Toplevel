﻿using System.ComponentModel.DataAnnotations;
using TomorrowsVoice_Toplevel.Models.Volunteering;

namespace TomorrowsVoice_Toplevel.Models.Events
{
    public class City
    {
        public int ID { get; set; }

        [Display(Name = "Name")]
        [Required(ErrorMessage = "Please enter a city name.")]
        [StringLength(50, ErrorMessage = "City name cannot be more than 50 characters long.")]
        public string Name { get; set; }

        [Display(Name = "Province")]
        [Required(ErrorMessage = "Please select a province.")]
        public Province Province { get; set; }
        public virtual ICollection<Chapter> Chapters { get; set; } = new HashSet<Chapter>();
        public virtual ICollection<CityEvent> CityEvents { get; set; } = new HashSet<CityEvent>();
    }
}