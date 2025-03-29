using System.ComponentModel.DataAnnotations;

namespace TomorrowsVoice_Toplevel.Models.Events
{
    public class Rehearsal : Auditable, IValidatableObject
    {
        public int ID { get; set; }

        [Display(Name = "Rehearsal Time")]
        public string Summary
        {
            get
            {
                return $"{RehearsalDate.ToLongDateString()} at {StartAt.ToString("h:mm tt").ToLower()}";
            }
        }

        public string TimeSummary
        {
            get
            {
                return $"{StartAt:hh:mm} to {EndAt:hh:mm tt}";
            }
        }

        [Display(Name = "Date")]
        [Required(ErrorMessage = "Please select a date for this rehearsal")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime RehearsalDate { get; set; }

        [Required(ErrorMessage = "Please select a start time for this rehearsal")]
        [Display(Name = "Start Time")]
        [DataType(DataType.Time)]
        public DateTime StartAt { get; set; }

        [Required(ErrorMessage = "Please select a end time for this rehearsal")]
        [Display(Name = "End Time")]
        [DataType(DataType.Time)]
        public DateTime EndAt { get; set; }

        [Display(Name = "Notes")]
        [StringLength(255)]
        [DataType(DataType.MultilineText)]
        public string? Note { get; set; }

        [Required(ErrorMessage = "Please enter the total number of singers enrolled.")]
        [Display(Name = "Singers Enrolled")]
        public int TotalSingers { get; set; }

        // status for archiving purposes
        public Status Status { get; set; } = Status.Active;

        [Required(ErrorMessage = "Please select a Director .")]
        [Display(Name = "Director")]
        public int DirectorID { get; set; }

        public Director? Director { get; set; }

        [Display(Name = "Chapter")]
        public int ChapterID { get; set; }

        public Chapter? Chapter { get; set; }

        public ICollection<RehearsalAttendance> RehearsalAttendances { get; set; } = new HashSet<RehearsalAttendance>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartAt >= EndAt)
            {
                yield return new ValidationResult("Start time must be earlier than end time.", ["StartAt"]);
            }
            /*if (RehearsalDate > DateTime.Today)
            {
                yield return new ValidationResult("Rehearsal Date should not be later than today's date.");
            }*/
        }
    }
}