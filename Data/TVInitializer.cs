using TomorrowsVoice_Toplevel.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace TomorrowsVoice_Toplevel.Data
{
	public class TVInitializer
	{
		/// <summary>
		/// Prepares the Database and seeds data as required
		/// </summary>
		/// <param name="serviceProvider">DI Container</param>
		/// <param name="DeleteDatabase">Delete the database and start from scratch</param>
		/// <param name="UseMigrations">Use Migrations or EnsureCreated</param>
		/// <param name="SeedSampleData">Add optional sample data</param>

		public static void Initialize(IServiceProvider serviceProvider,
		   bool DeleteDatabase = false, bool UseMigrations = true, bool SeedSampleData = true)
		{
			using (var context = new TVContext(
				serviceProvider.GetRequiredService<DbContextOptions<TVContext>>()))
			{
				//Refresh the database as per the parameter options

				#region Prepare the Database

				try
				{
					//Note: .CanConnect() will return false if the database is not there!
					if (DeleteDatabase || !context.Database.CanConnect())
					{
						context.Database.EnsureDeleted(); //Delete the existing version
						if (UseMigrations)
						{
							context.Database.Migrate(); //Create the Database and apply all migrations
						}
						else
						{
							context.Database.EnsureCreated(); //Create and update the database as per the Model
						}
					}
					else //The database is already created
					{
						if (UseMigrations)
						{
							context.Database.Migrate(); //Apply all migrations
						}
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.GetBaseException().Message);
				}

				#endregion Prepare the Database

				//Seed data needed for production and during development

				#region Seed Required Data

				try
				{
					// Add chapters using data from Tomorrow's Voices Website
					if (!context.Chapters.Any())
					{
						context.Chapters.AddRange(
							new Chapter
							{
								Name = "St. Catharines",
								Address = "188 Linwell Rd",
								City = "St. Catharines",
								Province = "ON",
								Postal = "L2N6N3"
							}, new Chapter
							{
								Name = "Hamilton",
								Address = "99 N Oval",
								City = "Hamilton",
								Province = "ON",
								Postal = "L8S3Z2"
							}, new Chapter
							{
								Name = "Toronto",
								Address = "452 College St",
								City = "Toronto",
								Province = "ON",
								Postal = "M6G 1A1"
							}, new Chapter
							{
								Name = "Saskatoon",
								Address = "35 – 22nd St. East",
								City = "Saskatoon",
								Province = "SK",
								Postal = "M6G 1A1"
							}, new Chapter
							{
								Name = "Vancouver"
							}, new Chapter
							{
								Name = "Surrey"
							});
						context.SaveChanges();
					}

					// Add Some Directors using data from Tomorrow's Voices Website
					if (!context.Directors.Any())
					{
						context.Directors.AddRange(
							new Director
							{
								FirstName = "Mendelt",
								LastName = "Hoekstra",
								Email = "mHoekstra@sample.com",
								Phone = "0000000000",
								ChapterID = 1
							}, new Director
							{
								FirstName = "Melissa",
								LastName = "Dutch",
								Email = "mDutch@sample.com",
								Phone = "1111111111",
								ChapterID = 2
							}, new Director
							{
								FirstName = "Anais",
								LastName = "Kelsey-Verdecchia",
								Email = "aKelsey-Verdecchia@sample.com",
								Phone = "2222222222",
								ChapterID = 3
							}, new Director
							{
								FirstName = "Brian",
								LastName = "Paul D.G.",
								Email = "bPaulDG@sample.com",
								Phone = "3333333333",
								ChapterID = 4
							}, new Director
							{
								FirstName = "Monique",
								LastName = "Hoekstra",
								Email = "mHoeskra1@sample.com",
								Phone = "4444444444",
								ChapterID = 5
							}, new Director
							{
								FirstName = "Frances",
								LastName = "Olson",
								Email = "fOlson@sample.com",
								Phone = "5555555555",
								ChapterID = 6
							});
						context.SaveChanges();
					}

					// Lists of First and Last Names
					Random rnd = new();
					string[] firstNames = new string[] { "Emma", "Liam", "Olivia", "Noah", "Ava", "Mason", "Sophia", "Ethan", "Mia", "Lucas", "Lyric", "Antoinette", "Kendal", "Vivian", "Ruth", "Jamison", "Emilia", "Natalee", "Yadiel", "Jakayla", "Lukas", "Moses", "Kyler", "Karla", "Chanel", "Tyler", "Camilla", "Quintin", "Braden", "Clarence" };
					string[] lastNames = new string[] { "Smith", "Johnson", "Brown", "Taylor", "Anderson", "White", "Harris", "Martin", "Lewis", "Walker", "Watts", "Randall", "Arias", "Weber", "Stone", "Carlson", "Robles", "Frederick", "Parker", "Morris", "Soto", "Bruce", "Orozco", "Boyer", "Burns", "Cobb", "Blankenship", "Houston", "Estes", "Atkins", "Miranda", "Zuniga", "Ward", "Mayo", "Costa", "Reeves", "Anthony", "Cook", "Krueger", "Crane", "Watts", "Little", "Henderson", "Bishop" };
					int firstNameCount = firstNames.Length;
					int lastNameCount = lastNames.Length;

					//Create 5 notes from Bacon ipsum
					string[] baconNotes = new string[] { "Bacon ipsum dolor amet meatball corned beef kevin, alcatra kielbasa biltong drumstick strip steak spare ribs swine. Pastrami shank swine leberkas bresaola, prosciutto frankfurter porchetta ham hock short ribs short loin andouille alcatra. Andouille shank meatball pig venison shankle ground round sausage kielbasa. Chicken pig meatloaf fatback leberkas venison tri-tip burgdoggen tail chuck sausage kevin shank biltong brisket.", "Sirloin shank t-bone capicola strip steak salami, hamburger kielbasa burgdoggen jerky swine andouille rump picanha. Sirloin porchetta ribeye fatback, meatball leberkas swine pancetta beef shoulder pastrami capicola salami chicken. Bacon cow corned beef pastrami venison biltong frankfurter short ribs chicken beef. Burgdoggen shank pig, ground round brisket tail beef ribs turkey spare ribs tenderloin shankle ham rump. Doner alcatra pork chop leberkas spare ribs hamburger t-bone. Boudin filet mignon bacon andouille, shankle pork t-bone landjaeger. Rump pork loin bresaola prosciutto pancetta venison, cow flank sirloin sausage.", "Porchetta pork belly swine filet mignon jowl turducken salami boudin pastrami jerky spare ribs short ribs sausage andouille. Turducken flank ribeye boudin corned beef burgdoggen. Prosciutto pancetta sirloin rump shankle ball tip filet mignon corned beef frankfurter biltong drumstick chicken swine bacon shank. Buffalo kevin andouille porchetta short ribs cow, ham hock pork belly drumstick pastrami capicola picanha venison.", "Picanha andouille salami, porchetta beef ribs t-bone drumstick. Frankfurter tail landjaeger, shank kevin pig drumstick beef bresaola cow. Corned beef pork belly tri-tip, ham drumstick hamburger swine spare ribs short loin cupim flank tongue beef filet mignon cow. Ham hock chicken turducken doner brisket. Strip steak cow beef, kielbasa leberkas swine tongue bacon burgdoggen beef ribs pork chop tenderloin.", "Kielbasa porchetta shoulder boudin, pork strip steak brisket prosciutto t-bone tail. Doner pork loin pork ribeye, drumstick brisket biltong boudin burgdoggen t-bone frankfurter. Flank burgdoggen doner, boudin porchetta andouille landjaeger ham hock capicola pork chop bacon. Landjaeger turducken ribeye leberkas pork loin corned beef. Corned beef turducken landjaeger pig bresaola t-bone bacon andouille meatball beef ribs doner. T-bone fatback cupim chuck beef ribs shank tail strip steak bacon." };

					// Randomly generate 60 singers and assign 10 to each chapter
					if (context.Singers.Count() < 60)
					{
						foreach (Chapter ch in context.Chapters)
						{
							for (int i = 0; i < 10; i++)
							{
								string contactFirst = firstNames[rnd.Next(firstNameCount)];
								string contactLast = lastNames[rnd.Next(lastNameCount)];

								Singer singer = new Singer
								{
									ContactName = $"{contactFirst} {contactLast}",
									Email = $"{contactFirst.Substring(0, 1).ToLower()}{contactLast}@gmail.com",
									Phone = $"{rnd.Next(999)}{rnd.Next(999)}{rnd.Next(9999)}",
									FirstName = firstNames[rnd.Next(firstNameCount)],
									LastName = contactLast,
									ChapterID = ch.ID
								};
								if (i % 2 == 0)
									singer.MiddleName = lastNames[rnd.Next(lastNameCount)][1].ToString().ToUpper();
								if (i % 3 == 0)
									singer.Note = baconNotes[rnd.Next(5)];
								try
								{
									context.Singers.Add(singer);
									context.SaveChanges();
								}
								catch (Exception)
								{
									context.Singers.Remove(singer);
								}
							}
						}
					}

					foreach (Director d in context.Directors)
					{
						DayOfWeek DOW = (DayOfWeek)rnd.Next(7);

						DateTime date = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DOW - 7);
						DateTime start = date.AddHours(rnd.Next(10, 18));
						if (rnd.Next(2) == 1)
							start.AddMinutes(30);
						DateTime end = start.AddHours(rnd.Next(1, 2));

						for (int i = 0; i < 10; i++)
						{
							Rehearsal rehearsal = new Rehearsal
							{
								RehearsalDate = date.AddDays(i * 7),
								StartTime = start,
								EndTime = end,
								DirectorID = d.ID,
								Director = d
							};
							try
							{
								context.Rehearsals.Add(rehearsal);
								context.SaveChanges();
							}
							catch (Exception)
							{
								context.Rehearsals.Remove(rehearsal);
							}
						}
					}

					foreach (Rehearsal rehearsal in context.Rehearsals)
					{
						if (rehearsal.Director != null)
						{
							foreach (Singer singer in context.Singers.Where(s => s.ChapterID == rehearsal.Director.ChapterID))
							{
								if (rnd.Next(4) >= 3)
								{
									RehearsalAttendance attendance = new RehearsalAttendance
									{
										SingerID = singer.ID,
										Singer = singer,
										RehearsalID = rehearsal.ID,
										Rehearsal = rehearsal,
									};
									try
									{
										context.RehearsalAttendances.Add(attendance);
										context.SaveChanges();
									}
									catch (Exception)
									{
										context.RehearsalAttendances.Remove(attendance);
									}
								}
							}
						}
					}

					// Generate some attendance logs NOT DONE NOT DONE

					//if (context.Rehearsals.Any())
					//{
					//    DateTime sample = DateTime.Today;

					//    context.Rehearsals.AddRange(
					//        new Rehearsal
					//        {
					//            RehearsalDate = sample.AddDays(rnd.Next(7)),
					//            StartTime = new DateTime(2025-01-01 )
					//            EndTime
					//            ChapterID
					//        });
					//}
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.GetBaseException().Message);
				}

				#endregion Seed Required Data
			}
		}
	}
}