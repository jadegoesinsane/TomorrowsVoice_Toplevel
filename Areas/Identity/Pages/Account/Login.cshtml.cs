// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Utilities;
using TomorrowsVoice_Toplevel.Models.Volunteering;
using TomorrowsVoice_Toplevel.Models;
using TomorrowsVoice_Toplevel.Models.Users;
using TomorrowsVoice_Toplevel.Models.Events;

namespace TomorrowsVoice_Toplevel.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
	{
		private readonly SignInManager<IdentityUser> _signInManager;
		private readonly ILogger<LoginModel> _logger;
		private readonly TVContext _context;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly UserLoginEventHandler _loginEventHandler;

		public LoginModel(SignInManager<IdentityUser> signInManager,
			ILogger<LoginModel> logger,
			UserManager<IdentityUser> userManager, UserLoginEventHandler loginEventHandler,
			TVContext context)
		{
			_signInManager = signInManager;
			_logger = logger;
			_userManager = userManager;
			_context = context;
			_loginEventHandler = loginEventHandler;
		}

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[BindProperty]
		public InputModel Input { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		public IList<AuthenticationScheme> ExternalLogins { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		public string ReturnUrl { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[TempData]
		public string ErrorMessage { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		public class InputModel
		{
			/// <summary>
			///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
			///     directly from your code. This API may change or be removed in future releases.
			/// </summary>
			[Required]
			[EmailAddress]
			public string Email { get; set; }

			/// <summary>
			///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
			///     directly from your code. This API may change or be removed in future releases.
			/// </summary>
			[Required]
			[DataType(DataType.Password)]
			public string Password { get; set; }

			/// <summary>
			///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
			///     directly from your code. This API may change or be removed in future releases.
			/// </summary>
			[Display(Name = "Remember me?")]
			public bool RememberMe { get; set; }
		}

		public async Task OnGetAsync(string returnUrl = null)
		{
			if (!string.IsNullOrEmpty(ErrorMessage))
			{
				ModelState.AddModelError(string.Empty, ErrorMessage);
			}

			returnUrl ??= Url.Content("~/");

			// Clear the existing external cookie to ensure a clean login process
			await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
			// Clear userName cookie
			HttpContext.Response.Cookies.Delete("userName");

			ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

			ReturnUrl = returnUrl;
		}

		public async Task<IActionResult> OnPostAsync(string returnUrl = null)
		{
			returnUrl ??= Url.Content("~/");

			ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

			if (ModelState.IsValid)
			{
				// This doesn't count login failures towards account lockout
				// To enable password failures to trigger account lockout, set lockoutOnFailure: true
				var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
				if (result.Succeeded)
				{

					// 获取用户信息
					var user = await _userManager.FindByEmailAsync(Input.Email);

					

					// 检查用户是否属于指定角色
					var roles = await _userManager.GetRolesAsync(user);
					if (roles.Contains("Admin") || roles.Contains("Planner") || roles.Contains("Volunteer"))
					{
						// 在用户成功登录后，调用邮件发送事件
						await _loginEventHandler.OnUserLoggedInAsync(user);
					}

					string summary = _context.Volunteers
						.Where(e => e.Email == Input.Email)
						.FirstOrDefault()?.NameFormatted ??
						_context.Directors
						.Where(e => e.Email == Input.Email)
						.FirstOrDefault()?.NameFormatted ?? 
						Input.Email;

					CookieHelper.CookieSet(HttpContext, "userName", summary, 3200);
					_logger.LogInformation("User logged in.");
					return LocalRedirect(returnUrl);
				}
				if (result.RequiresTwoFactor)
				{
					return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
				}
				if (result.IsLockedOut)
				{
					_logger.LogWarning("User account locked out.");
					return RedirectToPage("./Lockout");
				}
				else
				{
					Director director = _context.Directors.Where(d => d.Email == Input.Email).FirstOrDefault();
					Volunteer volunteer = _context.Volunteers.Where(e => e.Email == Input.Email).FirstOrDefault();
					var user = (Person)director ?? (Person)volunteer;
					if (user == null) //check if they are in the system
					{
						string msg = "Error: Account for " + Input.Email + " does not exist.";
						ModelState.AddModelError(string.Empty, msg);
					}
					else if (user != null && user.Status != Status.Active) //check if they are active
					{
						string msg = "Error: Account for login " + Input.Email + " is not active.";
						ModelState.AddModelError(string.Empty, msg);
					}
					else
					{
						ModelState.AddModelError(string.Empty, "Invalid login attempt.");
					}
					return Page();
				}
			}

			// If we got this far, something failed, redisplay form
			return Page();
		}
	}
}