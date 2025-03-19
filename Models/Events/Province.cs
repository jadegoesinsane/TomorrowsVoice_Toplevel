using System.ComponentModel.DataAnnotations;

namespace TomorrowsVoice_Toplevel.Models.Events
{
    public enum Province
    {
        Alberta,
        [Display(Name = "British Columbia")]
        BritishColumbia,
        Manitoba,
        [Display(Name = "New Brunswick")]
        NewBrunswick,
        [Display(Name = "Newfoundland and Labrador")]
        NewfoundlandAndLabrador,
        [Display(Name = "Nova Scotia")]
        Nova_Scotia,
        Ontario,
        [Display(Name = "Prince Edward Island")]
        PrinceEdwardIsland,
        Quebec,
        Saskatchewan,
        [Display(Name = "Northwest Territories")]
        NorthwestTerritories,
        Nunavut,
        Yukon
    }
}
