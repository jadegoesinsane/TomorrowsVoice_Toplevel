using System.ComponentModel.DataAnnotations;

namespace TomorrowsVoice_Toplevel.Models.Events
{
    public class RehearsalAttendance
    {
        public int ID { get; set; }
        public int SingerID { get; set; }

        public Singer? Singer { get; set; }

        public int RehearsalID { get; set; }
        public Rehearsal? Rehearsal { get; set; }
    }
}
