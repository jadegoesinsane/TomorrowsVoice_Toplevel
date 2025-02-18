﻿using Microsoft.EntityFrameworkCore;
using System.Numerics;
using TomorrowsVoice_Toplevel.Models;
using TomorrowsVoice_Toplevel.Models.Messaging;
using TomorrowsVoice_Toplevel.Models.Users;
using TomorrowsVoice_Toplevel.Models.Users.Account;
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

		// Accounts
		public DbSet<UserID> UserIDs { get; set; }

		public DbSet<Role> Roles { get; set; }

		// Chapters
		public DbSet<Director> Directors { get; set; }

		public DbSet<City> Cities { get; set; }
		public DbSet<Chapter> Chapters { get; set; }
		public DbSet<DirectorAvatar> DirectorAvatars { get; set; }
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
		public DbSet<VolunteerAvatar> VolunteerAvatars { get; set; }

		// Messaging DbSets
		public DbSet<Chat> Chats { get; set; }

		public DbSet<ChatUser> ChatUsers { get; set; }
		public DbSet<Message> Messages { get; set; }

		#endregion DbSets

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Message>()
				.Ignore(m => m.User);
			// Ensure Director & Volunteer have a sequential ID
			modelBuilder.Entity<UserID>().HasData(new UserID { ID = 1, NextID = 0 });

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

			// Many to Many Chat User PK
			modelBuilder.Entity<ChatUser>()
				.HasKey(cu => new { cu.ChatID, cu.UserID });
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

		public int GetNextID()
		{
			var userID = UserIDs.First();
			userID.NextID++;
			SaveChanges();
			return userID.NextID;
		}
	}
}