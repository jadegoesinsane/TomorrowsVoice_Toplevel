﻿using System.ComponentModel.DataAnnotations;
using System.Drawing.Imaging;
using TomorrowsVoice_Toplevel.Utilities;

namespace TomorrowsVoice_Toplevel.Models.Volunteering
{
    public class Event
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public string DateSummary
        {
            get
            {
                if (StartDate.Month == EndDate.Month & StartDate.Year == EndDate.Year)
                    return $"{StartDate.ToString("MMMM d")} - {EndDate.ToString("d, yyyy")}";
                else if (StartDate.Month != EndDate.Month & StartDate.Year == EndDate.Year)
                    return $"{StartDate.ToString("MMMM d")} - {EndDate.ToString("MMMM d, yyyy")}";
                else
                    return $"{StartDate.ToString("MMMM d, yyyy")} - {EndDate.ToString("MMMM d, yyyy")}";
            }
        }

        [Display(Name = "Start")]
        [Required(ErrorMessage = "Event requires a start date.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [Display(Name = "End")]
        [Required(ErrorMessage = "Event requires a end date.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }

        public string Descripion { get; set; }

        public string Location { get; set; }

        [Display(Name = "Background Colour")]
        public string BackgroundColour { get; set; } = ColourPalette.BrightColours["Blue"];

        [Display(Name = "Text Colour")]
        public string TextColour => ColourPalette.GetTextColour(BackgroundColour);

        [Display(Name = "Status")]
        public Status Status { get; set; } = Status.Active;

        public virtual ICollection<CityEvent> CityEvents { get; set; } = new HashSet<CityEvent>();
        public ICollection<Shift> Shifts { get; set; } = new HashSet<Shift>();
    }
}