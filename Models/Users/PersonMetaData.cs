﻿using System.ComponentModel.DataAnnotations;

namespace TomorrowsVoice_Toplevel.Models.Users
{
	public class PersonMetaData
	{
		[Display(Name = "Full Name")]
		public string NameFormatted
		{
			get
			{
				return FirstName
					+ (string.IsNullOrEmpty(MiddleName) ? " " :
						(" " + (char?)MiddleName[0] + ". ").ToUpper())
					+ LastName;
			}
		}

		[Display(Name = "Phone")]
		public string PhoneFormatted => "(" + Phone?.Substring(0, 3) + ") "
			+ Phone?.Substring(3, 3) + "-" + Phone?[6..];

		[Display(Name = "First Name")]
		[Required(ErrorMessage = "Please enter a first name.")]
		[MaxLength(50, ErrorMessage = "First name cannot be more than 50 characters long.")]
		public string FirstName { get; set; } = "";

		[Display(Name = "Middle Name")]
		[MaxLength(50, ErrorMessage = "Middle name cannot be more than 50 characters long.")]
		public string? MiddleName { get; set; }

		[Display(Name = "Last Name")]
		[Required(ErrorMessage = "Please enter a last name.")]
		[MaxLength(100, ErrorMessage = "Last name cannot be more than 100 characters long.")]
		public string LastName { get; set; } = "";

		[Required(ErrorMessage = "Please enter an email address.")]
		[RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please follow the correct email format test@email.com")]
		[StringLength(255)]
		[DataType(DataType.EmailAddress)]
		public virtual string Email { get; set; } = "";

		[Required(ErrorMessage = "Please enter a phone number.")]
		[RegularExpression("^\\d{10}$", ErrorMessage = "Please enter a valid 10-digit phone number (no spaces).")]
		[DataType(DataType.PhoneNumber)]
		[MaxLength(10)]
		public virtual string Phone { get; set; } = "";

		[Display(Name = "Status")]
		public Status Status { get; set; } = Status.Active;
	}
}