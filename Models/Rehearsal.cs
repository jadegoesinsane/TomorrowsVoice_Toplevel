using System.ComponentModel.DataAnnotations;


namespace TomorrowsVoice_Toplevel.Models
{
    public class Rehearsal
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage="Please select a start time for this rehearsal")]
        [Display(Name="Start Time")]
        [DataType(DataType.Time)]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "Please select a end time for this rehearsal")]
        [Display(Name = "End Time")]
        [DataType(DataType.Time)]
        public DateTime EndTime { get; set; }

        [Display(Name="Notes")]
        [StringLength(255)]
        [DataType(DataType.MultilineText)]
        public string? Note { get; set; }

        // Uncomment when Chapter and RehearsalAttendance classes are set up
        /*
        public int ChapterID { get; set; }
        public Chapter? Chapter { get; set; }

        public ICollection<ReherearsalAttendance> ReherearsalAttendances { get; set; } = new HashSet<RehearsalAttendance>();
        */
    }
}
