using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MusicStreamingService.Models;

namespace MusicStreamingService.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<DbSong> Songs { get; set; }
        public DbSet<DbUserSong> UserSongs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<DbSong>()
                .Property(e => e.Duration)
                .HasConversion(new TimeSpanToTicksConverter());

            base.OnModelCreating(builder);
        }

        public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt)
        {
            Database.EnsureCreated();

            if(Users!.Count() <= 0)
            {
                Users.Add(new User() { Username = "Faurazeko", Password = "Faurazeko" });

                SaveChanges();
            }
        }
    }
}
