using TomorrowsVoice_Toplevel.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

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
                                Postal = "L2N6N3",
                                DOW = "Wednesday"
                            }, new Chapter
                            {
                                Name = "Hamilton",
                                Address = "99 N Oval",
                                City = "Hamilton",
                                Province = "ON",
                                Postal = "L8S3Z2",
                                DOW = "Monday"
                            }, new Chapter
                            {
                                Name = "Toronto",
                                Address = "452 College St",
                                City = "Toronto",
                                Province = "ON",
                                Postal = "M6G 1A1",
                                DOW = "Tuesday"
                            }, new Chapter
                            {
                                Name = "Saskatoon",
                                Address = "35 – 22nd St. East",
                                City = "Saskatoon",
                                Province = "SK",
                                Postal = "M6G 1A1",
                                DOW = "Tuesday"
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