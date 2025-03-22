using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TomorrowsVoice_Toplevel.Models.Events;
using TomorrowsVoice_Toplevel.Models.Volunteering;

namespace TomorrowsVoice_Toplevel.Data
{
    public static class ApplicationDbInitializer
	{
		public static async void Initialize(IServiceProvider serviceProvider,
			bool UseMigrations = true, bool SeedSampleData = true)
		{
			#region Prepare the Database

			if (UseMigrations)
			{
				using (var context = new ApplicationDbContext(
				serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
				{
					try
					{
						//Create the database if it does not exist and apply the Migration
						context.Database.Migrate();
					}
					catch (Exception ex)
					{
						Debug.WriteLine(ex.GetBaseException().Message);
					}
				}
			}

			#endregion Prepare the Database

			#region Seed Sample Data

			if (SeedSampleData)
			{
				//Create Roles
				using (var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>())
				{
					try
					{
						string[] roleNames = { "Admin", "Director", "Planner", "Volunteer" };

						IdentityResult roleResult;
						foreach (var roleName in roleNames)
						{
							var roleExist = await roleManager.RoleExistsAsync(roleName);
							if (!roleExist)
							{
								roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
							}
						}
					}
					catch (Exception ex)
					{
						Debug.WriteLine(ex.GetBaseException().Message);
					}
				}

				//Create Users
				using (var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>())
				using (var context = new TVContext(
		serviceProvider.GetRequiredService<DbContextOptions<TVContext>>()))
				{
					try
					{
						string defaultPassword = "p@55w0rD";

						if (userManager.FindByEmailAsync("admin@outlook.com").Result == null)
						{
							IdentityUser user = new IdentityUser
							{
								UserName = "admin@outlook.com",
								Email = "admin@outlook.com",
								EmailConfirmed = true
							};

							IdentityResult result = userManager.CreateAsync(user, defaultPassword).Result;

							if (result.Succeeded)
							{
								userManager.AddToRoleAsync(user, "Admin").Wait();
							}
						}
						if (userManager.FindByEmailAsync("director@outlook.com").Result == null)
						{
							IdentityUser user = new IdentityUser
							{
								UserName = "director@outlook.com",
								Email = "director@outlook.com",
								EmailConfirmed = true
							};

							IdentityResult result = userManager.CreateAsync(user, defaultPassword).Result;

							if (result.Succeeded)
							{
								userManager.AddToRoleAsync(user, "Director").Wait();
							}
						}
						if (userManager.FindByEmailAsync("planner@outlook.com").Result == null)
						{
							IdentityUser user = new IdentityUser
							{
								UserName = "planner@outlook.com",
								Email = "planner@outlook.com",
								EmailConfirmed = true
							};

							IdentityResult result = userManager.CreateAsync(user, defaultPassword).Result;

							if (result.Succeeded)
							{
								userManager.AddToRoleAsync(user, "Planner").Wait();
							}
						}
						if (userManager.FindByEmailAsync("user@outlook.com").Result == null)
						{
							IdentityUser user = new IdentityUser
							{
								UserName = "user@outlook.com",
								Email = "user@outlook.com",
								EmailConfirmed = true
							};

							IdentityResult result = userManager.CreateAsync(user, defaultPassword).Result;
							//Not in any role
						}

						// Add Users

						foreach (Director d in context.Directors)
						{
							if (userManager.FindByEmailAsync(d.Email).Result == null)
							{
								IdentityUser user = new IdentityUser
								{
									UserName = d.Email,
									Email = d.Email,
									EmailConfirmed = true
								};

								IdentityResult result = userManager.CreateAsync(user, defaultPassword).Result;

								if (result.Succeeded)
								{
									userManager.AddToRoleAsync(user, "Director").Wait();
								}
							}
						}
						foreach (Volunteer v in context.Volunteers)
						{
							if (userManager.FindByEmailAsync(v.Email).Result == null)
							{
								IdentityUser user = new IdentityUser
								{
									UserName = v.Email,
									Email = v.Email,
									EmailConfirmed = true
								};

								IdentityResult result = userManager.CreateAsync(user, defaultPassword).Result;

								if (result.Succeeded)
								{
									userManager.AddToRoleAsync(user, "Volunteer").Wait();
								}
							}
						}
					}
					catch (Exception ex)
					{
						Debug.WriteLine(ex.GetBaseException().Message);
					}
				}
			}

			#endregion Seed Sample Data
		}
	}
}