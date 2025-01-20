using Microsoft.EntityFrameworkCore;
using System.Numerics;
using TomorrowsVoice_Toplevel.Models;

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
        public DbSet<Singer> Singers { get; set; }
        public DbSet<Rehearsal> Rehearsals { get; set; }
        public DbSet<RehearsalAttendance> RehearsalAttendances { get; set; }
        #endregion

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
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Rehearsal>()
                .HasMany<RehearsalAttendance>(r => r.RehearsalAttendances)
                .WithOne(r => r.Rehearsal)
                .HasForeignKey(r => r.RehearsalID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Rehearsal>()
                .HasOne(r => r.Director)
                .WithMany(r=>r.Rehearsals)
                .HasForeignKey(c => c.DirectorID)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            //OnBeforeSaving();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            //OnBeforeSaving();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        public DbSet<TomorrowsVoice_Toplevel.Models.Director> Director { get; set; } = default!;

        /*private void OnBeforeSaving()
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
        }*/

    }
}
