﻿using Azure.Core.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NToastNotify;
using TomorrowsVoice_Toplevel.CustomControllers;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.Models;
using TomorrowsVoice_Toplevel.Models.Events;
using TomorrowsVoice_Toplevel.Models.Volunteering;
using TomorrowsVoice_Toplevel.Utilities;
using TomorrowsVoice_Toplevel.ViewModels;

namespace TomorrowsVoice_Toplevel.Controllers
{
	[Authorize(Roles = "Admin,Planner,Volunteer")]
	public class VolunteerShiftController : ElephantController
	{
		private readonly TVContext _context;
		private readonly IMyEmailSender _emailSender;

		public VolunteerShiftController(TVContext context, IMyEmailSender emailSender, IToastNotification toastNotification) : base(context, toastNotification)
		{
			_emailSender = emailSender;
			_context = context;
		}

		public async Task<IActionResult> Index(int? VolunteerID)
		{
			ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "Volunteer");

			Volunteer? volunteer = null;
			if (User.IsInRole("Volunteer") || User.IsInRole("Planner"))
			{
				volunteer = await _context.Volunteers
				.Where(v => v.Email == User.Identity.Name)
				.AsNoTracking()
				.FirstOrDefaultAsync();
				VolunteerID = volunteer?.ID ?? null;
			}

			if (!VolunteerID.HasValue || volunteer == null)
			{
				return Redirect(ViewData["returnURL"].ToString());
			}
			var shifts = await _context.Shifts
				.Include(s => s.Colour)
				.Include(s => s.UserShifts)
					.ThenInclude(us => us.User)
				.Include(s => s.Event)
				.Where(s => s.UserShifts.Any(us => us.UserID == VolunteerID.GetValueOrDefault()))
				.OrderByDescending(s => s.StartAt)
				.AsNoTracking()
				.ToListAsync();

			ViewBag.Volunteer = volunteer;

			return View(shifts);
		}

		public async Task<IActionResult> SignUp(int? ShiftID)
		{
			ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "Event");

			try
			{
				if (!ShiftID.HasValue)
					throw new ArgumentNullException("Shift not found.");

				Volunteer? volunteer = await GetVolunteerFromUser();
				if (volunteer == null)
					throw new ArgumentNullException("Volunteer not found.");

				Shift? shift = await _context.Shifts
					.Include(s => s.UserShifts)
					.Include(s => s.Event)
					.FirstOrDefaultAsync(s => s.ID == ShiftID.Value);

				if (shift == null)
					throw new ArgumentNullException("Shift not found.");

				if (_context.UserShifts.Any(us => us.UserID == volunteer.ID && us.ShiftID == ShiftID.Value))
					throw new Exception("You are already signed up for this shift.");

				if (shift.UserShifts.Count >= shift.VolunteersNeeded)
					throw new Exception($"Sorry! That shift for {shift?.Event?.Name} is full.");

				var userShift = new UserShift
				{
					UserID = volunteer.ID,
					ShiftID = (int)ShiftID,
					StartAt = shift.StartAt,
					EndAt = shift.EndAt,
				};

				_context.Add(userShift);
				await _context.SaveChangesAsync();
				_toastNotification.AddSuccessToastMessage("Signed up for shift!");

				#region Email Volunteer

				var shift2 = await _context.Shifts
						.Include(d => d.Event)
						.AsNoTracking()
						.FirstOrDefaultAsync(d => d.ID == shift.ID);
				string Subject = "Sign Up shift ";

				string emailContent = $"You have Signed Up  for the shift {shift.TimeSummary}  of event {shift2.Event.Name}.  ";

				var volunteers = _context.Volunteers;

				int folksCount = 0;
				try
				{
					//Send a Notice.
					List<EmailAddress> folks = (from p in volunteers

												where p.ID == volunteer.ID
												select new EmailAddress
												{
													Name = p.NameFormatted,
													Address = p.Email
												}).ToList();
					folksCount = folks.Count;
					if (folksCount > 0)
					{
						var msg = new EmailMessage()
						{
							ToAddresses = folks,
							Subject = Subject,
							Content = "<p>" + emailContent
						};
						await _emailSender.SendToManyAsync(msg);
						ViewData["Message"] = "Message sent to " + folksCount + " Manager"
							+ ((folksCount == 1) ? "." : "s.");
					}
					else
					{
						ViewData["Message"] = "Message NOT sent!  No Manager.";
					}
				}
				catch (Exception ex)
				{
					string errMsg = ex.GetBaseException().Message;
					ViewData["Message"] = "Error: Could not send email message to the " + folksCount + " Client"
						+ ((folksCount == 1) ? "" : "s") + " in the list.";
				}

				#endregion Email Volunteer
			}
			catch (ArgumentNullException ex)
			{
				string msg = ex.GetBaseException().Message;
				_toastNotification.AddErrorToastMessage(msg);
			}
			catch (Exception ex)
			{
				string msg = ex.GetBaseException().Message;
				if (msg.Contains("UNIQUE") || msg.Contains("'UserID', 'ShiftID'"))
					_toastNotification.AddErrorToastMessage("You are already signed up for this shift!");
				else if (msg.Contains("SqliteException"))
					_toastNotification.AddErrorToastMessage("An error occurred while signing up for the shift. Please try again.");
				else
					_toastNotification.AddErrorToastMessage(msg);
			}
			return Redirect(ViewData["returnURL"].ToString());
		}

		public async Task<IActionResult> TrackPerformance(int id, int volunteerId)
		{
			var shift = await _context.Shifts
				.Include(g => g.UserShifts).ThenInclude(e => e.User)
				.FirstOrDefaultAsync(m => m.ID == id);

			if (shift == null)
			{
				return NotFound();
			}

			var enrollmentsVM = shift.UserShifts.Where(e => e.UserID == volunteerId).ToList().Select(e => new EnrollmentVM
			{
				ShiftID = e.ShiftID,
				UserID = e.UserID,
				Volunteer = e.User.NameFormatted,
				ShowOrNot = e.NoShow,
				StartAt = e.StartAt,
				EndAt = e.EndAt
			}).ToList();

			return PartialView("_TrackPerformance", enrollmentsVM);
		}

		[HttpPost]
		public async Task<IActionResult> UpdatePerformance([FromBody] List<Object> enrollments)
		{
			if (enrollments == null || enrollments.Count == 0)
			{
				return Json(new { success = false, message = "No data received." });
			}

			var enrollmentVMs = enrollments
				.Select(e => JsonConvert.DeserializeObject<EnrollmentVM>(e.ToString()))
				.ToList();

			try
			{
				foreach (var enrollmentVM in enrollmentVMs)
				{
					var volunteer = await _context.Volunteers.FirstOrDefaultAsync(m => m.ID == enrollmentVM.UserID);

					var userShifts = await _context.UserShifts.FirstOrDefaultAsync(e => e.ShiftID == enrollmentVM.ShiftID);

					var enrollment = await _context.UserShifts.Include(g => g.User)
						.FirstOrDefaultAsync(e => e.UserID == enrollmentVM.UserID && e.ShiftID == enrollmentVM.ShiftID);

					if (enrollmentVM.ShowOrNot == true && enrollmentVM.StartAt - enrollmentVM.EndAt != TimeSpan.Zero)
					{
						throw new InvalidOperationException("Cannot have work hours when marked as a No Show.");
					}

					if (enrollmentVM.ShowOrNot == false && enrollmentVM.StartAt >= enrollmentVM.EndAt)
					{
						throw new InvalidOperationException("Start time cannot be after end time when the volunteer shows up.");
					}

					if (enrollment != null)
					{
						enrollment.NoShow = enrollmentVM.ShowOrNot;
						enrollment.StartAt = enrollmentVM.StartAt;
						enrollment.EndAt = enrollmentVM.EndAt;
						enrollment.HourRecordedVolunteer= true;
					}
				}

				await _context.SaveChangesAsync();
				return Json(new { success = true, message = "Performance updated successfully." });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = "Error updating performance: " + ex.Message });
			}
		}

		public async Task<IActionResult> Signoff(int? ShiftID)
		{
			ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "VolunteerShift");

			Volunteer? volunteer = await GetVolunteerFromUser();
			Shift? shift = _context.Shifts.Where(s => s.ID == ShiftID).FirstOrDefault();

			if (volunteer == null)
				return Redirect(ViewData["returnURL"].ToString());

			var userShift = new UserShift
			{
				UserID = volunteer.ID,
				ShiftID = (int)ShiftID,
				StartAt = shift.StartAt,
				EndAt = shift.EndAt
			};

			_context.Remove(userShift);
			await _context.SaveChangesAsync();
			_toastNotification.AddSuccessToastMessage("Signed off for shift!");

			return Redirect(ViewData["returnURL"].ToString());
		}

		public async Task<IActionResult> SignOffShift(int volunteerId, int shiftId)
		{
			if (shiftId == null)
			{
				return NotFound();
			}

			var shift = await _context.Shifts
				.Include(s => s.Event)
				.FirstOrDefaultAsync(m => m.ID == shiftId);
			if (shift == null)
			{
				return NotFound();
			}
			ViewData["volunteerId"] = volunteerId;
			return View(shift);
		}

		[HttpPost, ActionName("SignOffShift")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> SignOffShiftConfirmed(int volunteerId, int shiftId)
		{
			var userShift = await _context.UserShifts
								   .FirstOrDefaultAsync(us => us.UserID == volunteerId && us.ShiftID == shiftId);

			var volunteer = _context.Volunteers
							.FirstOrDefault(v => v.ID == volunteerId);

			var Shift = _context.Shifts
								  .FirstOrDefault(v => v.ID == shiftId);

			// get info for toast notification
			string shiftDate = Shift.ShiftDate.ToLongDateString();
			string volunteerName = volunteer.NameFormatted;
			int eventID = _context.Shifts.Where(s => s.ID == shiftId).Select(s => s.EventID).FirstOrDefault();
			string eventName = _context.Events.Where(e => e.ID == eventID).Select(e => e.Name).FirstOrDefault();

			try
			{
				if (userShift != null)
				{
					_context.UserShifts.Remove(userShift);
					await _context.SaveChangesAsync();
					AddCancelledToast(shiftDate, volunteerName, eventName);
					//AddSuccessToast($"Shift {Shift.ShiftDate} successfully removed for volunteer {volunteer.NameFormatted}.");
					return RedirectToAction(nameof(Index));
				}
			}
			catch (DbUpdateException)
			{
				ModelState.AddModelError("", "Unable to delete record. Please try again.");
			}

			return RedirectToAction("VolunteerforUser", "Volunteer");
		}

		private async Task<Volunteer?> GetVolunteerFromUser()
		{
			return await _context.Volunteers
				.Where(v => v.Email == User.Identity.Name)
				.Include(v => v.UserShifts)
				.AsNoTracking()
				.FirstOrDefaultAsync();
		}
	}
}