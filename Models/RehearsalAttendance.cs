using System.ComponentModel.DataAnnotations;

namespace TomorrowsVoice_Toplevel.Models
{
    public class RehearsalAttendance
    {
        public int ID { get; set; }
        public int SignerID { get; set; }

        public Singer? Singer { get; set; }

        public int RehearsalID { get; set; }
        public Rehearsal? Rehearsal { get; set; }
    }
}
