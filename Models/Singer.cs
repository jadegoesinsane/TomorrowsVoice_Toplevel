using System.ComponentModel.DataAnnotations;

namespace TomorrowsVoice_Toplevel.Models
{
    public class Singer
    {
        public int ID { get; set; }

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "You cannot leave the first name blank.")]
        [MaxLength(50, ErrorMessage = "First name cannot be more than 50 characters long.")]
        public string FirstName { get; set; } = "";

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "You cannot leave the last name blank.")]
        [MaxLength(100, ErrorMessage = "Last name cannot be more than 100 characters long.")]
        public string LastName { get; set; } = "";

        [Display(Name = "Emergency Contact")]
        [Required(ErrorMessage = "You cannot leave the last name blank.")]
        [MaxLength(100, ErrorMessage = " Name cannot be more than 100 characters long.")]
        public string ContactName { get; set; } = "";

        [Display(Name = "Notes")]
        [StringLength(255)]
        [DataType(DataType.MultilineText)]
        public string? Note { get; set; }
        [Display(Name = "Chapter")]
        public int ChapterID { get; set; }
        public Chapter? Chapter { get; set; }
     
        public ICollection<RehearsalAttendance> RehearsalAttendances { get; set; } = new HashSet<RehearsalAttendance>();
    }
}
