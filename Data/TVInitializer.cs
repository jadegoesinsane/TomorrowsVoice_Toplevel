﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using TomorrowsVoice_Toplevel.Controllers;
using TomorrowsVoice_Toplevel.Models;
using TomorrowsVoice_Toplevel.Models.Events;
using TomorrowsVoice_Toplevel.Models.Users;
using TomorrowsVoice_Toplevel.Models.Volunteering;
using TomorrowsVoice_Toplevel.Utilities;
using TomorrowsVoice_Toplevel.ViewModels;
using Group = TomorrowsVoice_Toplevel.Models.Users.Group;

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

						//--------------------------------------------------------------------
						// Triggers
						// Chapters
						string sqlCmd = @"
                            CREATE TRIGGER SetChapterTimestampOnUpdate
                            AFTER UPDATE ON Chapters
                            BEGIN
                                UPDATE Chapters
                                SET RowVersion = randomblob(8)
                                WHERE rowid = NEW.rowid;
                            END;
                        ";
						context.Database.ExecuteSqlRaw(sqlCmd);

						sqlCmd = @"
                            CREATE TRIGGER SetChapterTimestampOnInsert
                            AFTER INSERT ON Chapters
                            BEGIN
                                UPDATE Chapters
                                SET RowVersion = randomblob(8)
                                WHERE rowid = NEW.rowid;
                            END;
                        ";
						context.Database.ExecuteSqlRaw(sqlCmd);

						// Directors
						sqlCmd = @"
                            CREATE TRIGGER SetDirectorTimestampOnUpdate
                            AFTER UPDATE ON Directors
                            BEGIN
                                UPDATE Directors
                                SET RowVersion = randomblob(8)
                                WHERE rowid = NEW.rowid;
                            END;
                        ";
						context.Database.ExecuteSqlRaw(sqlCmd);

						sqlCmd = @"
                            CREATE TRIGGER SetDirectorTimestampOnInsert
                            AFTER INSERT ON Directors
                            BEGIN
                                UPDATE Directors
                                SET RowVersion = randomblob(8)
                                WHERE rowid = NEW.rowid;
                            END;
                        ";
						context.Database.ExecuteSqlRaw(sqlCmd);

						// Rehearsals
						sqlCmd = @"
                            CREATE TRIGGER SetRehearsalTimestampOnUpdate
                            AFTER UPDATE ON Rehearsals
                            BEGIN
                                UPDATE Rehearsals
                                SET RowVersion = randomblob(8)
                                WHERE rowid = NEW.rowid;
                            END;
                        ";
						context.Database.ExecuteSqlRaw(sqlCmd);

						sqlCmd = @"
                            CREATE TRIGGER SetRehearsalTimestampOnInsert
                            AFTER INSERT ON Rehearsals
                            BEGIN
                                UPDATE Rehearsals
                                SET RowVersion = randomblob(8)
                                WHERE rowid = NEW.rowid;
                            END;
                        ";
						context.Database.ExecuteSqlRaw(sqlCmd);

						// Singers
						sqlCmd = @"
                            CREATE TRIGGER SetSingerTimestampOnUpdate
                            AFTER UPDATE ON Singers
                            BEGIN
                                UPDATE Singers
                                SET RowVersion = randomblob(8)
                                WHERE rowid = NEW.rowid;
                            END;
                        ";
						context.Database.ExecuteSqlRaw(sqlCmd);

						sqlCmd = @"
                            CREATE TRIGGER SetSingerTimestampOnInsert
                            AFTER INSERT ON Singers
                            BEGIN
                                UPDATE Singers
                                SET RowVersion = randomblob(8)
                                WHERE rowid = NEW.rowid;
                            END;
                        ";
						context.Database.ExecuteSqlRaw(sqlCmd);
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
					// Add cities using data from TV's Website
					if (!context.Cities.Any())
					{
						context.Cities.AddRange(
							new City
							{
								Name = "St. Catharines",
								Province = Province.Ontario
							},
							new City
							{
								Name = "Hamilton",
								Province = Province.Ontario
							},
							new City
							{
								Name = "Toronto",
								Province = Province.Ontario
							},
							new City
							{
								Name = "Saskatoon",
								Province = Province.Saskatchewan
							},
							new City
							{
								Name = "Vancouver",
								Province = Province.Saskatchewan
							},
							new City
							{
								Name = "Surrey",
								Province = Province.Saskatchewan
							});
						context.SaveChanges();
					}
					// Add chapters using data from Tomorrow's Voices Website
					if (!context.Chapters.Any())
					{
						context.Chapters.AddRange(
							new Chapter
							{
								CityID = context.Cities.FirstOrDefault(c => c.Name == "St. Catharines").ID,
								Address = "188 Linwell Rd",
								PostalCode = "L2N6N3"
							}, new Chapter
							{
								CityID = context.Cities.FirstOrDefault(c => c.Name == "Hamilton").ID,
								Address = "99 N Oval",
								PostalCode = "L8S3Z2"
							}, new Chapter
							{
								CityID = context.Cities.FirstOrDefault(c => c.Name == "Toronto").ID,
								Address = "452 College St",
								PostalCode = "M6G 1A1"
							}, new Chapter
							{
								CityID = context.Cities.FirstOrDefault(c => c.Name == "Saskatoon").ID,
								Address = "35 – 22nd St. East",
								PostalCode = "M6G 1A1"
							}, new Chapter
							{
								CityID = context.Cities.FirstOrDefault(c => c.Name == "Vancouver").ID,
								Address = "35 – 22nd St. East",
								PostalCode = "M6G 1A1"
							}, new Chapter
							{
								CityID = context.Cities.FirstOrDefault(c => c.Name == "Surrey").ID,
								Address = "35 – 22nd St. East",
								PostalCode = "M6G 1A1"
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
								Email = "director@outlook.com",
								Phone = "0000000000",
								ChapterID = 1,
								ID = context.GetNextID()
							}, new Director
							{
								FirstName = "Melissa",
								LastName = "Dutch",
								Email = "mDutch@sample.com",
								Phone = "1111111111",
								ChapterID = 2,
								ID = context.GetNextID()
							}, new Director
							{
								FirstName = "Anais",
								LastName = "Kelsey-Verdecchia",
								Email = "aKelsey-Verdecchia@sample.com",
								Phone = "2222222222",
								ChapterID = 3,
								ID = context.GetNextID()
							}, new Director
							{
								FirstName = "Brian",
								LastName = "Paul D.G.",
								Email = "bPaulDG@sample.com",
								Phone = "3333333333",
								ChapterID = 4,
								ID = context.GetNextID()
							}, new Director
							{
								FirstName = "Monique",
								LastName = "Hoekstra",
								Email = "mHoeskra1@sample.com",
								Phone = "4444444444",
								ChapterID = 5,
								ID = context.GetNextID()
							}, new Director
							{
								FirstName = "Frances",
								LastName = "Olson",
								Email = "fOlson@sample.com",
								Phone = "5555555555",
								ChapterID = 6,
								ID = context.GetNextID()
							});
						context.SaveChanges();
					}

					// Lists of First and Last Names
					Random rnd = new();
					string[] firstNames = new string[] { "Emma", "Liam", "Olivia", "Noah", "Ava", "Mason", "Sophia", "Ethan", "Mia", "Lucas", "Lyric", "Antoinette", "Kendal", "Vivian", "Ruth", "Jamison", "Emilia", "Natalee", "Yadiel", "Jakayla", "Lukas", "Moses", "Kyler", "Karla", "Chanel", "Tyler", "Camilla", "Quintin", "Braden", "Clarence" };
					string[] lastNames = new string[] { "Smith", "Johnson", "Brown", "Taylor", "Anderson", "White", "Harris", "Martin", "Lewis", "Walker", "Watts", "Randall", "Arias", "Weber", "Stone", "Carlson", "Robles", "Frederick", "Parker", "Morris", "Soto", "Bruce", "Orozco", "Boyer", "Burns", "Cobb", "Blankenship", "Houston", "Estes", "Atkins", "Miranda", "Zuniga", "Ward", "Mayo", "Costa", "Reeves", "Anthony", "Cook", "Krueger", "Crane", "Watts", "Little", "Henderson", "Bishop" };
					int firstNameCount = firstNames.Length;
					int lastNameCount = lastNames.Length;

					////Create 5 notes from Bacon ipsum
					//string[] baconNotes = new string[] { "Bacon ipsum dolor amet meatball corned beef kevin, alcatra kielbasa biltong drumstick strip steak spare ribs swine. Pastrami shank swine leberkas bresaola, prosciutto frankfurter porchetta ham hock short ribs short loin andouille alcatra. Andouille shank meatball pig venison shankle ground round sausage kielbasa. Chicken pig meatloaf fatback leberkas venison tri-tip burgdoggen tail chuck sausage kevin shank biltong brisket.", "Sirloin shank t-bone capicola strip steak salami, hamburger kielbasa burgdoggen jerky swine andouille rump picanha. Sirloin porchetta ribeye fatback, meatball leberkas swine pancetta beef shoulder pastrami capicola salami chicken. Bacon cow corned beef pastrami venison biltong frankfurter short ribs chicken beef. Burgdoggen shank pig, ground round brisket tail beef ribs turkey spare ribs tenderloin shankle ham rump. Doner alcatra pork chop leberkas spare ribs hamburger t-bone. Boudin filet mignon bacon andouille, shankle pork t-bone landjaeger. Rump pork loin bresaola prosciutto pancetta venison, cow flank sirloin sausage.", "Porchetta pork belly swine filet mignon jowl turducken salami boudin pastrami jerky spare ribs short ribs sausage andouille. Turducken flank ribeye boudin corned beef burgdoggen. Prosciutto pancetta sirloin rump shankle ball tip filet mignon corned beef frankfurter biltong drumstick chicken swine bacon shank. Buffalo kevin andouille porchetta short ribs cow, ham hock pork belly drumstick pastrami capicola picanha venison.", "Picanha andouille salami, porchetta beef ribs t-bone drumstick. Frankfurter tail landjaeger, shank kevin pig drumstick beef bresaola cow. Corned beef pork belly tri-tip, ham drumstick hamburger swine spare ribs short loin cupim flank tongue beef filet mignon cow. Ham hock chicken turducken doner brisket. Strip steak cow beef, kielbasa leberkas swine tongue bacon burgdoggen beef ribs pork chop tenderloin.", "Kielbasa porchetta shoulder boudin, pork strip steak brisket prosciutto t-bone tail. Doner pork loin pork ribeye, drumstick brisket biltong boudin burgdoggen t-bone frankfurter. Flank burgdoggen doner, boudin porchetta andouille landjaeger ham hock capicola pork chop bacon. Landjaeger turducken ribeye leberkas pork loin corned beef. Corned beef turducken landjaeger pig bresaola t-bone bacon andouille meatball beef ribs doner. T-bone fatback cupim chuck beef ribs shank tail strip steak bacon." };

					// Sample Singer Notes
					string[] singerNotes = new string[]
					{
						"This singer has severe allergies to peanuts and tree nuts. Exposure to these allergens can cause an anaphylactic reaction, requiring immediate administration of an epinephrine auto-injector (EpiPen) and emergency medical attention. Ensure the child avoids all foods that might contain these allergens.",
						"The child has asthma and may require an inhaler during physical activity or in response to environmental triggers like pollen, dust, or cold air. Watch for signs of difficulty breathing, wheezing, or persistent coughing, and ensure they have access to their inhaler at all times.",
						"The child has Type 1 Diabetes and requires careful monitoring of blood sugar levels. They may need to check their blood sugar before meals, after physical activity, or if they feel unwell. Be aware of symptoms of hypoglycemia (e.g., shakiness, confusion, sweating) and provide fast-acting glucose if needed.",
						"The child has epilepsy and is at risk of seizures. Common signs include sudden staring, uncontrolled movements, or loss of consciousness. If a seizure occurs, ensure the child is in a safe position, do not restrain them, and time the seizure. Notify emergency services if the seizure lasts longer than 5 minutes or if it's the first seizure you observe.",
						"The child has severe allergies to bee stings and certain pollens. A bee sting could result in an anaphylactic reaction, requiring immediate use of an epinephrine auto-injector (EpiPen). Symptoms of exposure to other allergens, like hay fever or skin irritation, should be reported, and measures to reduce exposure (e.g., staying indoors during peak pollen times) should be taken."
					};

					// Sample attendance notes
					string[] noteString = new string[] {
						"Several singers were sick due a stomach bug",
						"Every singer attended practice this week!",
						"2 singers are unable to continue coming to rehearsals due to hockey season starting back up",
						"4 singers did not make it to practice due to bad driving conditions",
						"Singers were very rowdy today and struggled to focus"
					};

					// Randomly generate 60 singers and assign 10 to each chapter
					if (!context.Singers.Any())
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
									Phone = $"{rnd.Next(100, 1000)}{rnd.Next(100, 1000)}{rnd.Next(1000, 10000)}",
									FirstName = firstNames[rnd.Next(firstNameCount)],
									LastName = contactLast,
									ChapterID = ch.ID
								};
								if (i % 2 == 0)
									singer.MiddleName = lastNames[rnd.Next(lastNameCount)][1].ToString().ToUpper();
								if (i % 3 == 0)
									singer.Note = singerNotes[rnd.Next(5)];
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

					if (!context.Rehearsals.Any())
					{
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
									StartAt = start,
									EndAt = end,
									DirectorID = d.ID,
									Director = d,
									ChapterID = d.ChapterID,
									TotalSingers = context.Singers.Where(s => s.ChapterID == d.ChapterID && s.Status == Status.Active).Count()
								};
								if (i % 3 == 0)
								{
									rehearsal.Note = noteString[rnd.Next(5)];
								}
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
					}
					if (!context.RehearsalAttendances.Any())
					{
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
					}
					// Volunteering Seeding

					// Volunteers
					// Randomly generate 50 singers
					if (!context.Volunteers.Any())
					{
						context.Volunteers.AddRange(
							new Volunteer
							{
								FirstName = "Cave",
								LastName = "Johnson",
								Email = $"planner@outlook.com",
								Phone = $"2221112222",
								//HoursVolunteered = 0,
								ParticipationCount = 0,
								absences = 0,
								TotalWorkDuration = TimeSpan.Zero,
								ID = 1000
							});

						for (int i = 0; i < 50; i++)
						{
							string first = firstNames[rnd.Next(firstNameCount)];
							string last = lastNames[rnd.Next(lastNameCount)];
							int hoursVolunteered = rnd.Next(50, 100);
							Volunteer volunteer = new Volunteer
							{
								Email = $"{first.Substring(0, 1).ToLower()}{last}@gmail.com",
								Phone = $"{rnd.Next(100, 1000)}{rnd.Next(100, 1000)}{rnd.Next(1000, 10000)}",
								FirstName = first,
								LastName = last,
								//HoursVolunteered = 0,
								ParticipationCount = 0,
								absences = 0,
								TotalWorkDuration = TimeSpan.Zero,
								ID = context.GetNextID()
							};
							if (i % 2 == 0)
								volunteer.MiddleName = lastNames[rnd.Next(lastNameCount)][1].ToString().ToUpper();

							try
							{
								context.Volunteers.Add(volunteer);
								context.SaveChanges();
							}
							catch (Exception)
							{
								context.Volunteers.Remove(volunteer);
							}
						}
					}

					// Events
					if (!context.Events.Any())
					{
						var events = new List<Event>
						{
							new Event
							{
								Name = "St.C Gift Wrapping - 2024",
								StartDate = new DateTime(2024, 11, 29),
								EndDate = new DateTime(2024, 12, 22),
								Descripion = "Join us to help wrap gifts for those in need!",
								Location = "Pen Center, St. Catharines",
								Status = Status.Active
							},
							new Event
							{
								Name = "St.C Gift Wrapping - 2025",
								StartDate = new DateTime(2025, 11, 30),
								EndDate = new DateTime(2025, 12, 21),
								Descripion = "Join us to help wrap gifts for those in need!",
								Location = "Pen Center, St. Catharines",
								Status = Status.Active
							},
							new Event
							{
								Name = "Ham Bake Sale - 2025",
								StartDate = new DateTime(2025, 03, 20),
								EndDate = new DateTime(2025, 03, 25),
								Descripion = "We will be selling homemade baked goods",
								Location = "Eastgate Square, Hamilton",
								Status = Status.Active
							},
							new Event
							{
								Name = "Tor Bake Sale - 2024",
								StartDate = new DateTime(2024, 7, 10),
								EndDate = new DateTime(2024, 7, 15),
								Descripion = "We will be selling homemade baked goods",
								Location = "Eaton Centre, Toronto",
								Status = Status.Active
							}
						};
						context.Events.AddRange(events);
						context.SaveChanges();

						//CityEvents
						context.CityEvents.AddRange(new List<CityEvent>
						{
							new CityEvent { EventID = events[0].ID, CityID = context.Cities.FirstOrDefault(c => c.Name == "St. Catharines").ID },
							new CityEvent { EventID = events[3].ID, CityID = context.Cities.FirstOrDefault(c => c.Name == "Toronto").ID },
							new CityEvent { EventID = events[2].ID, CityID = context.Cities.FirstOrDefault(c => c.Name == "Hamilton").ID },
							new CityEvent { EventID = events[1].ID, CityID = context.Cities.FirstOrDefault(c => c.Name == "St. Catharines").ID },
						});

						context.SaveChanges();
					}

					// Shifts
					if (!context.Shifts.Any())
					{
						var shifts = new List<Shift>();
						foreach (var @event in context.Events)
						{
							if (@event.Name == "St.C Gift Wrapping - 2024")
							{
								List<DateTime> dates = new List<DateTime>
							{
								new DateTime(2024, 11, 29),
								new DateTime(2024, 12, 2),
								new DateTime(2024, 12, 3),
								new DateTime(2024, 12, 6),
								new DateTime(2024, 12, 7),
								new DateTime(2024, 12, 8),
								new DateTime(2024, 12, 11),
								new DateTime(2024, 12, 12),
								new DateTime(2024, 12, 16),
								new DateTime(2024, 12, 17),
								new DateTime(2024, 12, 20),
								new DateTime(2024, 12, 21),
								new DateTime(2024, 12, 22)
							};

								List<(TimeSpan Start, TimeSpan End)> times = new List<(TimeSpan, TimeSpan)>
							{
								(new TimeSpan(10, 0, 0), new TimeSpan(14, 0, 0)),   // 10am to 2pm
							};

								foreach (var date in dates)
								{
									foreach (var time in times)
									{
										if (date.DayOfWeek == DayOfWeek.Sunday && time.Start.Hours >= 18)
											continue;

										Shift shift = new Shift
										{
											ShiftDate = date,
											EventID = @event.ID,
											StartAt = date.Add(time.Start),
											EndAt = date.Add(time.End),
											VolunteersNeeded = 5
										};
										shifts.Add(shift);
									}
								}
							}
							else if (@event.Name == "St.C Gift Wrapping - 2025")
							{
								List<DateTime> dates = new List<DateTime>
							{
								new DateTime(2025, 11, 30),
								new DateTime(2025, 12, 2),
								new DateTime(2025, 12, 3),
								new DateTime(2025, 12, 6),
								new DateTime(2025, 12, 7),
								new DateTime(2025, 12, 8),
								new DateTime(2025, 12, 11),
								new DateTime(2025, 12, 12),
								new DateTime(2025, 12, 16),
								new DateTime(2025, 12, 17),
								new DateTime(2025, 12, 20),
								new DateTime(2025, 12, 21),
								new DateTime(2025, 12, 22)
							};

								List<(TimeSpan Start, TimeSpan End)> times = new List<(TimeSpan, TimeSpan)>
							{
								(new TimeSpan(10, 0, 0), new TimeSpan(14, 0, 0)),   // 10am to 2pm
							};

								foreach (var date in dates)
								{
									foreach (var time in times)
									{
										if (@event.Shifts.Any(s => s.StartAt == date.Add(time.Start)))
											continue;

										Shift shift = new Shift
										{
											ShiftDate = date,
											EventID = @event.ID,
											StartAt = date.Add(time.Start),
											EndAt = date.Add(time.End),
											VolunteersNeeded = 5
										};
										shifts.Add(shift);
									}
								}
							}
							else if (@event.Name == "Ham Bake Sale - 2025")
							{
								List<DateTime> dates = new List<DateTime>
							{
								new DateTime(2025, 03, 20),
								new DateTime(2025, 03, 21),
								new DateTime(2025, 03, 22),
								new DateTime(2025, 03, 23),
								new DateTime(2025, 03, 24),
								new DateTime(2025, 03, 25),
							};

								List<(TimeSpan Start, TimeSpan End)> times = new List<(TimeSpan, TimeSpan)>
							{
								(new TimeSpan(10, 0, 0), new TimeSpan(14, 0, 0)),   // 8am to 12pm
								(new TimeSpan(14, 0, 0), new TimeSpan(18, 0, 0)),  // 12pm to 4pm
							};

								foreach (var date in dates)
								{
									foreach (var time in times)
									{
										Shift shift = new Shift
										{
											ShiftDate = date,
											EventID = @event.ID,
											StartAt = date.Add(time.Start),
											EndAt = date.Add(time.End),
											VolunteersNeeded = 2
										};
										shifts.Add(shift);
									}
								}
							}
							else if (@event.Name == "Tor Bake Sale - 2024")
							{
								List<DateTime> dates = new List<DateTime>
							{
								new DateTime(2024, 07, 10),
								new DateTime(2024, 07, 11),
								new DateTime(2024, 07, 12),
								new DateTime(2024, 07, 13),
								new DateTime(2024, 07, 14),
								new DateTime(2024, 07, 15),
							};

								List<(TimeSpan Start, TimeSpan End)> times = new List<(TimeSpan, TimeSpan)>
							{
								(new TimeSpan(11, 0, 0), new TimeSpan(18, 0, 0)),  // 12pm to 4pm
							};

								foreach (var date in dates)
								{
									foreach (var time in times)
									{
										Shift shift = new Shift
										{
											ShiftDate = date,
											EventID = @event.ID,
											StartAt = date.Add(time.Start),
											EndAt = date.Add(time.End),
											VolunteersNeeded = 2
										};
										shifts.Add(shift);
									}
								}
							}
							else if (@event.ID == 4)
							{
								List<DateTime> dates = new List<DateTime>
							{
								new DateTime(2025, 07, 10),
								new DateTime(2025, 07, 11),
								new DateTime(2025, 07, 12),
								new DateTime(2025, 07, 13),
								new DateTime(2025, 07, 14),
								new DateTime(2025, 07, 15),
							};

								List<(TimeSpan Start, TimeSpan End)> times = new List<(TimeSpan, TimeSpan)>
							{
								(new TimeSpan(10, 0, 0), new TimeSpan(14, 0, 0)),   // 8am to 12pm
								(new TimeSpan(14, 0, 0), new TimeSpan(18, 0, 0)),  // 12pm to 4pm
							};

								foreach (var date in dates)
								{
									foreach (var time in times)
									{
										Shift shift = new Shift
										{
											ShiftDate = date,
											EventID = @event.ID,
											StartAt = date.Add(time.Start),
											EndAt = date.Add(time.End),
											VolunteersNeeded = 2
										};
										shifts.Add(shift);
									}
								}
							}
						}

						context.Shifts.AddRange(shifts);
						context.SaveChanges();

						//User Shifts
						if (!context.UserShifts.Any())
						{
							var users = context.Volunteers.ToList().Cast<Volunteer>().ToArray();

							foreach (Shift shift in context.Shifts)
							{
								int need = rnd.Next(shift.VolunteersNeeded + 1);
								while (need > 0)
								{
									rnd.Shuffle<Volunteer>(users);
									var user = users.First();

									var existing = context.UserShifts
										.AsNoTracking()
										.FirstOrDefault(us => us.UserID == user.ID && us.ShiftID == shift.ID);
									if (existing == null)
									{
										UserShift userShift = new UserShift
										{
											UserID = user.ID,
											User = user,
											ShiftID = shift.ID,
											Shift = shift,
											StartAt = TimeSpan.Zero,
											EndAt = TimeSpan.Zero
										};

										var volunteer = context.Volunteers.FirstOrDefault(v => v.ID == user.ID);
										if (volunteer != null)
											volunteer.UserShifts.Add(userShift);

										shift.UserShifts.Add(userShift);

										try
										{
											context.UserShifts.Add(userShift);
											context.SaveChanges();
											need -= 1;
										}
										catch (Exception)
										{
											context.UserShifts.Remove(userShift);
										}
									}
									else
									{
										Debug.WriteLine($"UserShift already exists!");
									}
								}
							}
						}
					}

					// Add Groups
					if (!context.Groups.Any())
					{
						List<Group> groups = new List<Group>
						{
							new Group { Name = "Highschool Students" },
							new Group { Name = "Brock Volunteers" },
							new Group { Name = "McMaster Volunteers" },
							new Group { Name = "Event Planners" },
						};
						context.Groups.AddRange(groups);
						context.SaveChanges();
					}
					// Add VolunteerGroups
					if (!context.VolunteerGroups.Any())
					{
						var volunteerGroups = new List<VolunteerGroup>();
						var volunteers = context.Volunteers.ToArray();

						foreach (var group in context.Groups.Where(g => g.Name != "Event Planner"))
						{
							rnd.Shuffle<Volunteer>(volunteers);

							for (int i = 0; i < rnd.Next(5, 15); i++)
							{
								var volunteerGroup = new VolunteerGroup
								{
									Group = group,
									GroupID = group.ID,
									Volunteer = volunteers[i],
									VolunteerID = volunteers[i].ID
								};

								try
								{
									group.VolunteerGroups.Add(volunteerGroup);
									volunteers[i].VolunteerGroups.Add(volunteerGroup);
									context.VolunteerGroups.Add(volunteerGroup);
									context.SaveChanges();
								}
								catch (Exception)
								{
									context.VolunteerGroups.Remove(volunteerGroup);
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.GetBaseException().Message);
				}

				#endregion Seed Required Data
			}
		}

		private static void AddUser(string email, string? role, IServiceProvider serviceProvider)
		{
		}
	}
}