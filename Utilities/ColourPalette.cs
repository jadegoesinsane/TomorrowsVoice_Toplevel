using System.ComponentModel.DataAnnotations;

namespace TomorrowsVoice_Toplevel.Utilities
{
    public static class ColourPalette
    {
        public static readonly Dictionary<string, string> BrightColours = new Dictionary<string, string>
        {
            { "Blue", "#467ECE" }, // Default
            { "Purple", "#9944bc" },
            { "Red", "#d3162b" },
            { "Brown", "#804205" },
            { "Magenta", "#aa394f" }
        };

        public static readonly Dictionary<string, string> PastelColours = new Dictionary<string, string>
        {
            { "Pink", "#F6CBDF" },
            { "Green", "#D7E3C0" },
            { "Yellow", "#f5e0ac" },
            { "Light Blue", "#BFD6E9" },
            { "Light Purple", "#d8cbe7" }
        };

        public static string GetTextColour(string colour)
        {
            if (BrightColours.ContainsValue(colour))
                return "#FFFFFF";
            else if (PastelColours.ContainsValue(colour))
                return "#000000";
            else
                return "#000000";
        }
    }
}