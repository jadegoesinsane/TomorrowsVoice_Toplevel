using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TomorrowsVoice_Toplevel.Data;
using TomorrowsVoice_Toplevel.ViewModels;
using Microsoft.EntityFrameworkCore;


namespace TomorrowsVoice_Toplevel.Utilities
{
	public class UserLoginEventHandler
	{
		private readonly IMyEmailSender _emailSender;
		private readonly TimeSpan _startTime = new TimeSpan(8, 0, 0);  // 10:00 AM
		private readonly TimeSpan _endTime = new TimeSpan(21, 0, 0);
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public UserLoginEventHandler(IServiceScopeFactory serviceScopeFactory, IMyEmailSender emailSender)
		{
			_emailSender = emailSender; _serviceScopeFactory = serviceScopeFactory;
		}

		public async Task OnUserLoggedInAsync(IdentityUser user)
		{
			
				var currentTime = DateTime.Now.TimeOfDay;

				if (currentTime >= _startTime && currentTime <= _endTime)
				{

					using (var scope = _serviceScopeFactory.CreateScope())
					{
						var dbContext = scope.ServiceProvider.GetRequiredService<TVContext>();

						var today = DateTime.Now.Date;
						var shifts = dbContext.Shifts
							.Include(v => v.Event)
							.Include(v => v.UserShifts)
								.ThenInclude(v => v.User)
							.Where(s => s.ShiftDate >= DateTime.Today && s.ShiftDate <= DateTime.Today.AddDays(2))
							 .ToList();


						var volunteers = dbContext.Volunteers.Include(v => v.UserShifts);

						foreach (var shift in shifts)
						{


							foreach (var userShift in shift.UserShifts)
							{
								if (userShift.SentNotice == false)
								{
									var volunteer = dbContext.Volunteers.Where(p => p.ID == userShift.UserID);
									string subject = "Notification of Upcoming Shift";
									string emailContent = $"You have a shift scheduled for event {shift.Event.Name} on {shift.ShiftDate.ToShortDateString()} from {shift.StartAt.ToShortTimeString()} to {shift.EndAt.ToShortTimeString()}. Please make sure to be present on that day.  ";

									try
									{
										var folks = volunteers
											.Where(p => p.ID == userShift.UserID)
											.Select(p => new EmailAddress
											{
												Name = p.NameFormatted,
												Address = p.Email
											})
											.ToList();

										if (folks.Any())
										{
											var msg = new EmailMessage()
											{
												ToAddresses = folks,
												Subject = subject,
												Content = "<p>" + emailContent + "</p>"
											};


											await _emailSender.SendToManyAsync(msg);
											userShift.SentNotice = true;


										}
									}
									catch (Exception ex)
									{

									}

								}

							}

							await dbContext.SaveChangesAsync();
						}
					}
				}
			
		}
	}

}
