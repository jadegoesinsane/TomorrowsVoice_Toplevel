using System.ComponentModel.DataAnnotations;

namespace TomorrowsVoice_Toplevel.Models
{
    public class Singer : Person
    {
        public int ID { get; set; }

        [Display(Name = "Singer")]
        public string Summary
        {
            get { return NameFormatted; }
        }

        [Display(Name = "Emergency Contact")]
        [Required(ErrorMessage = "You cannot leave the last name blank.")]
        [MaxLength(100, ErrorMessage = " Name cannot be more than 100 characters long.")]
        public string ContactName { get; set; } = "";

        [Display(Name = "Notes")]
        [StringLength(255)]
        [DataType(DataType.MultilineText)]
        public string? Note { get; set; }

        [Display(Name = "Chapter")]
        [Required(ErrorMessage = "You must select a Chapter")]
        public int ChapterID { get; set; }

        public Chapter? Chapter { get; set; }

        public ICollection<RehearsalAttendance> RehearsalAttendances { get; set; } = new HashSet<RehearsalAttendance>();
    }
}
