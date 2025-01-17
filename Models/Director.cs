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
    }
}
