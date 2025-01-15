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