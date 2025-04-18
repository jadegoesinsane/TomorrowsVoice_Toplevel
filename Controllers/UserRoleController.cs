﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TomorrowsVoice_Toplevel.CustomControllers;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models.Events;
using TomorrowsVoice_Toplevel.Utilities;
using TomorrowsVoice_Toplevel.ViewModels;

namespace TomorrowsVoice_Toplevel.Controllers
{
	[Authorize(Roles = "Admin")]
	public class UserRoleController : CognizantController
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<IdentityUser> _userManager;

		public UserRoleController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		// GET: User
		public async Task<IActionResult> Index(string? searchString, int? page, int? pageSizeID)
		{
			var users = (from u in _context.Users
							   .OrderBy(u => u.UserName)
						 select new UserVM
						 {
							 Id = u.Id,
							 UserName = u.UserName,
							 // getting the users roles manually here to keep users as a queryable to work with paging
                             UserRoles = (from r in _context.Roles
                                          join ur in _context.UserRoles on r.Id equals ur.RoleId
                                          where ur.UserId == u.Id
                                          select r.Name).ToList()

                         });

			// filter
			if (!String.IsNullOrEmpty(searchString))
			{
				users = users.Where(u => u.UserName.ToUpper().Contains(searchString.ToUpper()));
			}

			//foreach (var u in users)
			//{
			//	var user = await _userManager.FindByIdAsync(u.Id);
			//	u.UserRoles = (List<string>)await _userManager.GetRolesAsync(user);
			//	//Note: we needed the explicit cast above because GetRolesAsync() returns an IList<string>
			//};

			// paging
			int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
			ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
			var pagedData = await PaginatedList<UserVM>.CreateAsync(users.AsNoTracking(), page ?? 1, pageSize);

			return View(pagedData);
		}

		// GET: Users/Edit/5
		public async Task<IActionResult> Edit(string id)
		{
			if (id == null)
			{
				return new BadRequestResult();
			}
			var _user = await _userManager.FindByIdAsync(id);//IdentityRole
			if (_user == null)
			{
				return NotFound();
			}
			UserVM user = new UserVM
			{
				Id = _user.Id,
				UserName = _user.UserName,
				UserRoles = (List<string>)await _userManager.GetRolesAsync(_user)
			};
			PopulateAssignedRoleData(user);
			return View(user);
		}

		// POST: Users/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(string Id, string[] selectedRoles)
		{
			var _user = await _userManager.FindByIdAsync(Id);
			if (_user == null)
			{
				return NotFound();
			}

			UserVM user = new UserVM
			{
				Id = _user.Id,
				UserName = _user.UserName,
				UserRoles = (List<string>)await _userManager.GetRolesAsync(_user)
			};

			try
			{
			
				var currentUserId = _userManager.GetUserId(User);
				bool isEditingSelf = _user.Id == currentUserId;
				bool wasAdmin = user.UserRoles.Contains("Admin");
				bool isRemovingAdmin = !selectedRoles.Contains("Admin");

				if (isEditingSelf && wasAdmin && isRemovingAdmin)
				{
					var admins = await _userManager.GetUsersInRoleAsync("Admin");
					if (admins.Count == 1) 
					{
						ModelState.AddModelError("", "You are the last administrator.There must be at least one administrator.");
						PopulateAssignedRoleData(user);
						return View(user);
					}
				}

				await UpdateUserRoles(selectedRoles, user);
				return RedirectToAction("Index");
			}
			catch (Exception)
			{
				ModelState.AddModelError(string.Empty, "Unable to save changes.");
			}

			PopulateAssignedRoleData(user);
			return View(user);
		}

		private void PopulateAssignedRoleData(UserVM user)
		{//Prepare checkboxes for all Roles
			var allRoles = _context.Roles;
			var currentRoles = user.UserRoles;
			var viewModel = new List<RoleVM>();
			foreach (var r in allRoles)
			{
				viewModel.Add(new RoleVM
				{
					RoleId = r.Id,
					RoleName = r.Name,
					Assigned = currentRoles.Contains(r.Name)
				});
			}
			ViewBag.Roles = viewModel;
		}

		private async Task UpdateUserRoles(string[] selectedRoles, UserVM userToUpdate)
		{
			var UserRoles = userToUpdate.UserRoles;//Current roles use is in
			var _user = await _userManager.FindByIdAsync(userToUpdate.Id);//IdentityUser

			if (selectedRoles == null)
			{
				//No roles selected so just remove any currently assigned
				foreach (var r in UserRoles)
				{
					await _userManager.RemoveFromRoleAsync(_user, r);
				}
			}
			else
			{
				//At least one role checked so loop through all the roles
				//and add or remove as required

				//We need to do this next line because foreach loops don't always work well
				//for data returned by EF when working async.  Pulling it into an IList<>
				//first means we can safely loop over the colleciton making async calls and avoid
				//the error 'New transaction is not allowed because there are other threads running in the session'
				IList<IdentityRole> allRoles = _context.Roles.ToList<IdentityRole>();

				foreach (var r in allRoles)
				{
					if (selectedRoles.Contains(r.Name))
					{
						if (!UserRoles.Contains(r.Name))
						{
							await _userManager.AddToRoleAsync(_user, r.Name);
						}
					}
					else
					{
						if (UserRoles.Contains(r.Name))
						{
							await _userManager.RemoveFromRoleAsync(_user, r.Name);
						}
					}
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_context.Dispose();
				_userManager.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}