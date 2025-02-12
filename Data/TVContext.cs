using Microsoft.EntityFrameworkCore;
using System.Numerics;
using TomorrowsVoice_Toplevel.Models;
using TomorrowsVoice_Toplevel.Models.Volunteering;

namespace TomorrowsVoice_Toplevel.Data
{
	public class TVContext : DbContext
	{
		//To give access to IHttpContextAccessor for Audit Data with IAuditable
		private readonly IHttpContextAccessor _httpContextAccessor;

		//Property to hold the UserName value
		public string UserName
		{
			get; private set;
		}

		public TVContext(DbContextOptions<TVContext> options, IHttpContextAccessor httpContextAccessor)
			 : base(options)
		{
			_httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
			if (_httpContextAccessor.HttpContext != null)
			{
				//We have a HttpContext, but there might not be anyone Authenticated
				UserName = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";
			}
			else
			{
				//No HttpContext so seeding data
				UserName = "Seed Data";
			}
		}

		public TVContext(DbContextOptions<TVContext> options)
			: base(options)
		{
			_httpContextAccessor = null!;
			UserName = "Seed Data";
		}

		#region DbSets

		public DbSet<Chapter> Chapters { get; set; }
		public DbSet<Director> Directors { get; set; }
		public DbSet<DirectorDocument> DirectorDocuments { get; set; }
		public DbSet<UploadedFile> UploadedFiles { get; set; }
		public DbSet<Singer> Singers { get; set; }
		public DbSet<Rehearsal> Rehearsals { get; set; }
		public DbSet<RehearsalAttendance> RehearsalAttendances { get; set; }

		// Volunteer DbSets
		public DbSet<Event> Events { get; set; }

		public DbSet<ChapterEvent> ChapterEvents { get; set; }
		public DbSet<Shift> Shifts { get; set; }
		public DbSet<Volunteer> Volunteers { get; set; }
		public DbSet<VolunteerShift> VolunteerShifts { get; set; }

		#endregion DbSets

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Chapter>()
				.HasIndex(c => c.Name)
				.IsUnique();

			modelBuilder.Entity<Chapter>()
				.HasMany(d => d.Directors)
				.WithOne(d => d.Chapter)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Singer>()
				.HasOne(s => s.Chapter)
				.WithMany(s => s.Singers)
				.HasForeignKey(s => s.ChapterID)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Singer>()
				.HasMany<RehearsalAttendance>(s => s.RehearsalAttendances)
				.WithOne(s => s.Singer)
				.HasForeignKey(s => s.SingerID)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<Rehearsal>()
				.HasMany<RehearsalAttendance>(r => r.RehearsalAttendances)
				.WithOne(r => r.Rehearsal)
				.HasForeignKey(r => r.RehearsalID)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<Rehearsal>()
				.HasOne(r => r.Director)
				.WithMany(r => r.Rehearsals)
				.HasForeignKey(c => c.DirectorID)
				.OnDelete(DeleteBehavior.Restrict);

			// unique instructor email
			modelBuilder.Entity<Director>()
				.HasIndex(d => d.Email)
				.IsUnique();

			// prevent cascade delete from director to chapter
			modelBuilder.Entity<Director>()
				.HasOne<Chapter>(d => d.Chapter)
				.WithMany(c => c.Directors)
				.HasForeignKey(d => d.ChapterID)
				.OnDelete(DeleteBehavior.Restrict);

			// Rehearsal No Director Time Overlap (Assuming one rehearsal a day) only if rehearsal is Active
			modelBuilder.Entity<Rehearsal>()
				.HasIndex(r => new { r.DirectorID, r.RehearsalDate, r.Status })
				.IsUnique()
				.HasFilter("[Status] = 0");

			modelBuilder.Entity<Singer>()
			.HasIndex(p => new { p.FirstName, p.LastName, p.Email })
			.IsUnique();

			// Many to Many Chapter Event PK
			modelBuilder.Entity<ChapterEvent>()
				.HasKey(ce => new { ce.ChapterID, ce.EventID });

			// Many to Many Volunteer Shift PK
			modelBuilder.Entity<VolunteerShift>()
				.HasKey(vs => new { vs.VolunteerID, vs.ShiftID });
		}

		public override int SaveChanges(bool acceptAllChangesOnSuccess)
		{
			OnBeforeSaving();
			return base.SaveChanges(acceptAllChangesOnSuccess);
		}

		public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
		{
			OnBeforeSaving();
			return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
		}

		private void OnBeforeSaving()
		{
			var entries = ChangeTracker.Entries();
			foreach (var entry in entries)
			{
				if (entry.Entity is IAuditable trackable)
				{
					var now = DateTime.UtcNow;
					switch (entry.State)
					{
						case EntityState.Modified:
							trackable.UpdatedOn = now;
							trackable.UpdatedBy = UserName;
							break;

						case EntityState.Added:
							trackable.CreatedOn = now;
							trackable.CreatedBy = UserName;
							trackable.UpdatedOn = now;
							trackable.UpdatedBy = UserName;
							break;
					}
				}
			}
		}
	}
}