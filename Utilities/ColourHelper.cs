using System.Drawing;

namespace TomorrowsVoice_Toplevel.Utilities
{
	public class ColourHelper
	{
		// https://www.w3.org/TR/2008/REC-WCAG20-20081211/#relativeluminancedef
		public static double RelativeLuminance(Color color)
		{
			double R = color.R / 255d, G = color.G / 255d, B = color.B / 255d;
			double r = R <= 0.03928 ? R / 12.92 : Math.Pow(((R + 0.055) / 1.055), 2.4),
				g = G <= 0.03928 ? G / 12.92 : Math.Pow(((G + 0.055) / 1.055), 2.4),
				b = B <= 0.03928 ? B / 12.92 : Math.Pow(((B + 0.055) / 1.055), 2.4);

			return 0.2126 * r + 0.7152 * g + 0.0722 * b;
		}

		// https://www.w3.org/TR/2008/REC-WCAG20-20081211/#contrast-ratiodef
		public static double ContrastRatio(Color first, Color second)
		{
			double first_lum = RelativeLuminance(first);
			double second_lum = RelativeLuminance(second);

			return first_lum > second_lum ? (first_lum + 0.05) / (second_lum + 0.05) : (second_lum + 0.05) / (first_lum + 0.05);
		}
	}
}