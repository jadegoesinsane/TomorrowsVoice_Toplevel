using System.ComponentModel.DataAnnotations;
using TomorrowsVoice_Toplevel.Utilities;
using System.Drawing;

namespace TomorrowsVoice_Toplevel.Models
{
	public class ColourScheme
	{
		public int ID { get; set; }

		[StringLength(55)]
		[Required(ErrorMessage = "Must set a colour scheme name.")]
		public string Name { get; set; }

		[Display(Name = "Background Colour Hex Value")]
		[Required(ErrorMessage = "Must set background colour.")]
		public string BackgroundColour
		{
			get => _backgroundColour;
			set => _backgroundColour = value?.ToUpper();
		}
		private string _backgroundColour;

		[Display(Name = "Text Colour Hex Value")]
		[Required(ErrorMessage = "Must set text colour.")]
		public string TextColour
		{
			get => _textColour;
			set => _textColour = value?.ToUpper();
		}
		private string _textColour;

		[Display(Name = "Border Colour Hex Value")]
		[Required(ErrorMessage = "Must set border colour.")]
		public string BorderColour
		{
			get => _borderColour;
			set => _borderColour = value?.ToUpper();
		}
		private string _borderColour;

		public string GetRating(string rating, double? ratio = null, bool isLargeText = false)
		{
			if (!ratio.HasValue)
				ratio = GetContrastRatio();

			double AA = isLargeText ? 3.0 : 4.5;
			double AAA = isLargeText ? 4.5 : 7.0;

			if (rating == "AA") return ratio >= AA ? "PASS" : "FAIL";
			if (rating == "AAA") return ratio >= AAA ? "PASS" : "FAIL";
			return "Invalid";
		}

		public double GetContrastRatio()
		{
			return ColourHelper.ContrastRatio(ColorTranslator.FromHtml(TextColour), ColorTranslator.FromHtml(BackgroundColour));
		}
	}
}