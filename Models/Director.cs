using System.ComponentModel.DataAnnotations;

namespace TomorrowsVoice_Toplevel.Models
{
    public class Director : Person
    {
        public int ID { get; set; }

        [Display(Name = "Director")]
        public string Summary
        {
            get { return NameFormatted; }
        }
        [Display(Name = "Chapter")]
        [Required(ErrorMessage = "You must select a Chapter")]
        public int ChapterID { get; set; }

        public Chapter? Chapter { get; set; }

    }
}
