﻿using Microsoft.EntityFrameworkCore;
using System.Numerics;
using TomorrowsVoice_Toplevel.Models;
using TomorrowsVoice_Toplevel.Models.Events;
using TomorrowsVoice_Toplevel.Models.Users;
using TomorrowsVoice_Toplevel.Models.Volunteering;

namespace TomorrowsVoice_Toplevel.Data
{
	public class TVContext : DbContext
	{
		//To give access to IHttpContextAccessor for Audit Data with IAuditable
		private readonly IHttpContextAccessor _httpContextAccessor;

		private bool _isLogging = false;
		private string[] _auditable = new[] { "CreatedBy", "CreatedOn", "UpdatedBy", "UpdatedOn" };

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

		public DbSet<ColourScheme> ColourSchemes { get; set; }

		// Accounts
		//public DbSet<UserID> UserIDs { get; set; }

		//public DbSet<User> Users { get; set; }

		// Chapters
		public DbSet<Director> Directors { get; set; }

		public DbSet<City> Cities { get; set; }
		public DbSet<Chapter> Chapters { get; set; }
		public DbSet<DirectorDocument> DirectorDocuments { get; set; }
		public DbSet<UploadedFile> UploadedFiles { get; set; }
		public DbSet<Singer> Singers { get; set; }
		public DbSet<Rehearsal> Rehearsals { get; set; }
		public DbSet<RehearsalAttendance> RehearsalAttendances { get; set; }

		// Volunteer DbSets
		public DbSet<Event> Events { get; set; }

		public DbSet<CityEvent> CityEvents { get; set; }
		public DbSet<Shift> Shifts { get; set; }
		public DbSet<Volunteer> Volunteers { get; set; }
		public DbSet<UserShift> UserShifts { get; set; }
		public DbSet<Group> Groups { get; set; }
		public DbSet<VolunteerGroup> VolunteerGroups { get; set; }

		// Logging DbSet
		public DbSet<Transaction> Transactions { get; set; }

		#endregion DbSets

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			//modelBuilder.Entity<User>()
			//	.HasDiscriminator<string>("UserType")
			//	.HasValue<Volunteer>("Volunteer")
			//	.HasValue<Director>("Director");
			// Ensure Director & Volunteer have a sequential ID
			//modelBuilder.Entity<UserID>().HasData(new UserID { ID = 1, NextID = 0 });

			modelBuilder.Entity<City>()
				.HasIndex(c => new { c.Province, c.Name })
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

			// Unique volunteer email
			modelBuilder.Entity<Volunteer>()
				.HasIndex(v => v.Email)
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
			modelBuilder.Entity<CityEvent>()
				.HasKey(ce => new { ce.CityID, ce.EventID });

			// Many to Many User Shift PK
			modelBuilder.Entity<UserShift>()
				.HasKey(us => new { us.UserID, us.ShiftID });

			// Many to Many User Shift PK
			modelBuilder.Entity<VolunteerGroup>()
				.HasKey(vg => new { vg.VolunteerID, vg.GroupID });

			modelBuilder.Entity<Event>()
			.HasMany<Shift>(s => s.Shifts)
			.WithOne(s => s.Event)
			.HasForeignKey(s => s.EventID)
			.OnDelete(DeleteBehavior.Cascade);

			// Delete UserShifts if a volunteer is deleted
			modelBuilder.Entity<Volunteer>()
				.HasMany<UserShift>(s => s.UserShifts)
				.WithOne(s => s.User)
				.HasForeignKey(s => s.UserID)
				.OnDelete(DeleteBehavior.Cascade);

			// Delete UserShifts if a shift is deleted
			modelBuilder.Entity<Shift>()
				.HasMany<UserShift>(s => s.UserShifts)
				.WithOne(s => s.Shift)
				.HasForeignKey(s => s.ShiftID)
				.OnDelete(DeleteBehavior.Cascade);

			// Cannot have duplicate colour names
			modelBuilder.Entity<ColourScheme>()
				.HasIndex(cs => cs.Name)
				.IsUnique();
		}

		public override int SaveChanges(bool acceptAllChangesOnSuccess)
		{
			var changes = CaptureChanges(out var tempIDs);
			var result = base.SaveChanges(acceptAllChangesOnSuccess);
			LogChanges(changes, tempIDs);
			return result;
		}

		public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
		{
			var changes = CaptureChanges(out var tempIDs);
			var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
			LogChanges(changes, tempIDs);
			return result;
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

		private List<Transaction> CaptureChanges(out Dictionary<long, int> tempIDs)
		{
			OnBeforeSaving();
			var entries = ChangeTracker.Entries();
			var logs = new List<Transaction>();
			tempIDs = new Dictionary<long, int>();
			foreach (var entry in entries)
			{
				if (entry.Entity is IAuditable trackable)
				{
					var now = DateTime.UtcNow;
					switch (entry.State)
					{
						case EntityState.Modified:
							var originalValues = entry.OriginalValues;
							var currentValues = entry.CurrentValues;
							foreach (var property in entry.OriginalValues.Properties)
							{
								if (_auditable.Contains(property.Name))
									continue;
								var originalValue = originalValues[property]?.ToString();
								var currentValue = currentValues[property]?.ToString();
								if (originalValue != currentValue)
								{
									logs.Add(new Transaction
									{
										ItemID = (int)entry.Property("ID").CurrentValue,
										ChangedBy = UserName,
										ChangeType = "UPDATE",
										ChangeTimestamp = now,
										OldValue = originalValue,
										NewValue = currentValue,
										Property = property.Name,
										ItemType = entry.Entity.GetType().Name
									});
								}
							}
							break;

						case EntityState.Added:
							var ID = trackable.CreatedOn.Value.Ticks;
							tempIDs[ID] = 0;
							logs.Add(new Transaction
							{
								ItemID = ID,
								ChangedBy = UserName,
								ChangeType = "INSERT",
								ChangeTimestamp = now,
								OldValue = null,
								NewValue = entry.CurrentValues.ToObject().ToString(),
								Property = entry.Entity.GetType().Name,
								ItemType = entry.Entity.GetType().Name
							});
							break;

						case EntityState.Deleted:
							var origin = entry.OriginalValues.ToObject().ToString();
							logs.Add(new Transaction
							{
								ItemID = (int)entry.Property("ID").CurrentValue,
								ChangedBy = UserName,
								ChangeType = "DELETE",
								ChangeTimestamp = now,
								OldValue = origin,
								NewValue = null,
								Property = entry.Entity.GetType().Name,
								ItemType = entry.Entity.GetType().Name
							});
							break;
					}
				}
			}
			return logs;
		}

		private void LogChanges(List<Transaction> changes, Dictionary<long, int> tempIDs)
		{
			var entries = ChangeTracker.Entries();

			foreach (var entry in entries)
			{
				if (entry.Entity is IAuditable trackable)
				{
					if (entry.State == EntityState.Unchanged)
					{
						var actualID = (int)entry.Property("ID").CurrentValue;
						var createdOn = ((IAuditable)entry.Entity).CreatedOn.Value.Ticks;
						if (tempIDs.ContainsKey(createdOn))
							tempIDs[createdOn] = actualID;
					}
				}
			}
			foreach (var change in changes.Where(c => c.ChangeType == "INSERT" && tempIDs.ContainsKey(c.ItemID)))
				change.ItemID = tempIDs[change.ItemID];

			if (changes.Any())
			{
				_isLogging = true;
				Transactions.AddRange(changes);
				base.SaveChanges();
				_isLogging = false;
			}
		}

		//public int GetNextID()
		//{
		//	var userID = UserIDs.First();
		//	userID.NextID++;
		//	SaveChanges();
		//	return userID.NextID;
		//}
	}
}